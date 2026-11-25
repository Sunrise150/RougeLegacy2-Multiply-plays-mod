using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 远端分身的受击盒（v2.1.0）。
    ///
    /// 问题：对面玩家的分身只是一个外观克隆（RemotePlayer_x），克隆时把所有 Collider2D /
    /// Rigidbody2D 都剥掉了（否则会被物理推着乱跑）。结果是怪物子弹、近战、尖刺全部从
    /// 分身身上穿过去——观感上"根本打不到人"。
    ///
    /// 做法：
    ///   1) 在分身下挂一个 trigger 的 BoxCollider2D 子物体 "Hurtbox"，层用 Body_Hitbox(21)
    ///      ——和真身的身体判定同一层，所以敌方 hitbox 的碰撞矩阵天然就对它生效。
    ///      不加 Rigidbody2D：敌方 HitboxController 自带 Rigidbody2D，触发检测照样成立，
    ///      少一个刚体就少一份物理开销。
    ///   2) 被碰到时从对面的 HitboxInfo → HitboxController → DamageObj 里读真实伤害值，
    ///      本地先放命中特效（立刻有"打到了"的反馈），再把伤害转发给分身背后的真身。
    ///   3) 同一来源 0.3s 内只转发一次、全局 0.1s 一次，避免连发/持续接触刷包。
    ///
    /// 只对敌方（Enemy / EnemyProjectile / Hazard）生效；玩家自己的武器层不处理，
    /// 协作模式里不做友伤。
    /// </summary>
    public class RemoteHurtbox : MonoBehaviour
    {
        /// <summary>这个分身背后的真人玩家 id。</summary>
        internal int OwnerId = -1;

        private Transform _hurtbox;
        private BoxCollider2D _box;

        private float _nextHitAllowed;
        private readonly Dictionary<int, float> _sourceCooldown = new Dictionary<int, float>();

        private SpriteRenderer[] _sprites;
        private bool _flashing;

        private static readonly Color HitFlashColor = new Color(1f, 0.72f, 0.72f, 1f);

        /// <summary>给分身挂受击盒（幂等，可重复调用）。</summary>
        internal static void Attach(GameObject remoteRoot, int ownerId)
        {
            if (remoteRoot == null) return;
            try
            {
                var box = remoteRoot.GetComponent<RemoteHurtbox>();
                if (box == null) box = remoteRoot.AddComponent<RemoteHurtbox>();
                box.OwnerId = ownerId;
                box.EnsureBox();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [HURTBOX] Attach 异常: " + Plugin.PatchError(ex));
            }
        }

        private void EnsureBox()
        {
            if (_box != null) return;

            // 尺寸取本地真身的碰撞盒（世界坐标 Rect）→ 转到分身本地空间。
            // 取不到就用 RL2 玩家的常见尺寸兜底。
            Vector3 center = new Vector3(0f, 0.85f, 0f);
            Vector3 size = new Vector3(0.85f, 1.7f, 0.1f);
            try
            {
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc != null)
                {
                    Rect r = pc.CollisionBounds;
                    if (r.width > 0.05f && r.height > 0.05f)
                    {
                        Vector3 wc = new Vector3(r.center.x, r.center.y, transform.position.z);
                        center = transform.InverseTransformPoint(wc);
                        center.z = 0f;
                        // 分身的 localScale 带朝向翻面（±），先归一化再乘回真实宽度
                        float sx = Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.x));
                        float sy = Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.y));
                        size = new Vector3(r.width / sx, r.height / sy, 0.1f);
                    }
                }
            }
            catch { /* 本地角色还没进场景：用兜底尺寸，后面 Update 会再校一次 */ }

            var go = new GameObject("Hurtbox");
            go.layer = (int)LayerType.Body_Hitbox;
            var t = go.transform;
            t.SetParent(transform, false);
            t.localPosition = center;
            t.localScale = Vector3.one;

            _box = go.AddComponent<BoxCollider2D>();
            _box.isTrigger = true;
            _box.size = new Vector2(Mathf.Max(0.2f, size.x), Mathf.Max(0.2f, size.y));
            _box.offset = Vector2.zero;

            _hurtbox = t;
            Plugin.Log.LogInfo("[RL2MP] [HURTBOX] 分身 " + gameObject.name + " 受击盒已就绪 size=("
                + _box.size.x.ToString("F2") + "," + _box.size.y.ToString("F2") + ") owner=P" + OwnerId);
        }

        private void Update()
        {
            // 分身刚创建时本地角色可能还没进场景，尺寸是兜底值——这里在拿到真身后补正一次
            if (_box == null || _dimsFixed) return;
            var ps = PlayerSync.Instance;
            var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
            if (pc == null) return;

            try
            {
                Rect r = pc.CollisionBounds;
                if (r.width <= 0.05f || r.height <= 0.05f) return;
                Vector3 wc = new Vector3(r.center.x, r.center.y, transform.position.z);
                Vector3 center = transform.InverseTransformPoint(wc);
                center.z = 0f;
                float sx = Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.x));
                float sy = Mathf.Max(0.01f, Mathf.Abs(transform.lossyScale.y));
                _hurtbox.localPosition = center;
                _box.size = new Vector2(r.width / sx, r.height / sy);
                _dimsFixed = true;
            }
            catch { }
        }

        private bool _dimsFixed;

        private void OnTriggerEnter2D(Collider2D other)
        {
            TryHit(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            // 持续接触伤害（怪物身体压过来）：原生 repeatHitDuration 是 0.5s，这里同样节流
            TryHit(other);
        }

        private void TryHit(Collider2D other)
        {
            if (other == null) return;
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            if (!PlayerHurtSync.Active) return;

            // 已经倒地/本端就是这个人（不该发生）都不处理
            var ps = PlayerSync.Instance;
            if (ps == null || OwnerId < 0) return;
            if (ps.RemoteDowned.Contains(OwnerId)) return;

            float now = Time.unscaledTime;
            if (now < _nextHitAllowed) return;

            GameObject root;
            float damage;
            float stun;
            if (!ResolveDamage(other, out root, out damage, out stun)) return;
            if (damage <= 0.01f) return;

            int srcKey = root != null ? root.GetInstanceID() : other.GetInstanceID();
            float last;
            if (_sourceCooldown.TryGetValue(srcKey, out last) && now - last < 0.3f) return;
            _sourceCooldown[srcKey] = now;
            if (_sourceCooldown.Count > 64) _sourceCooldown.Clear();   // 防泄漏

            _nextHitAllowed = now + 0.1f;

            // 本地即时反馈：命中火花 + 分身闪一下。真身端的伤害数字/屏幕效果由转发那头负责。
            PlayLocalHitFx(other);

            PlayerHurtSync.SendHurt(OwnerId, damage, stun, other.transform.position);
        }

        /// <summary>从敌方 collider 上解析"这一下打多少"。</summary>
        private bool ResolveDamage(Collider2D other, out GameObject root, out float damage, out float stun)
        {
            root = null;
            damage = 0f;
            stun = 0f;
            try
            {
                HitboxInfo hbInfo = other.GetComponent<HitboxInfo>();
                if (hbInfo == null) hbInfo = other.GetComponentInParent<HitboxInfo>();

                IHitboxController hbc = hbInfo != null ? hbInfo.HitboxController : null;
                if (hbc != null) root = hbc.RootGameObject;
                if (root == null) root = other.transform.root.gameObject;
                if (root == null) return false;

                // 只吃敌方的伤害（协作模式不做友伤）
                bool hostile = root.CompareTag("Enemy")
                               || root.CompareTag("EnemyProjectile")
                               || root.CompareTag("Hazard");
                if (!hostile)
                {
                    // 有些弹丸根挂在别的节点上，退一步按弹丸自身 tag 判断
                    var proj = other.GetComponentInParent<Projectile_RL>();
                    if (proj != null && proj.CompareTag("EnemyProjectile")) root = proj.gameObject;
                    else return false;
                }

                IDamageObj dmg = hbc != null ? hbc.DamageObj : null;
                if (dmg == null) dmg = root.GetComponentInParent<IDamageObj>();
                if (dmg == null) dmg = other.GetComponentInParent<IDamageObj>();
                if (dmg == null) return false;

                damage = dmg.ActualDamage;
                if (damage <= 0f) damage = dmg.BaseDamage;
                stun = dmg.ActualStunStrength;
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [HURTBOX] 解析伤害异常: " + ex.Message);
                return false;
            }
        }

        private void PlayLocalHitFx(Collider2D other)
        {
            try
            {
                Vector3 point = other != null ? other.transform.position : transform.position;
                if (EffectManager.Instance != null)
                {
                    EffectManager.PlayEffect(gameObject, null, "HitSparksSlash_Effect",
                        point, 0f, EffectStopType.Gracefully);
                }
            }
            catch { /* 特效库缺 Effect 时不要影响转发 */ }

            if (!_flashing) StartCoroutine(FlashRoutine());
        }

        private IEnumerator FlashRoutine()
        {
            _flashing = true;
            try
            {
                // 缓存失效（白块升级成整树克隆时旧 renderer 被销毁）就重建一次
                if (_sprites == null || _sprites.Length == 0 || AllNull(_sprites))
                    _sprites = GetComponentsInChildren<SpriteRenderer>(true);

                var saved = new Color[_sprites.Length];
                for (int i = 0; i < _sprites.Length; i++)
                {
                    if (_sprites[i] == null) continue;
                    saved[i] = _sprites[i].color;
                    _sprites[i].color = HitFlashColor;
                }
                yield return new WaitForSecondsRealtime(0.07f);
                for (int i = 0; i < _sprites.Length; i++)
                {
                    if (_sprites[i] == null) continue;
                    _sprites[i].color = saved[i];
                }
            }
            finally
            {
                _flashing = false;
            }
        }

        private static bool AllNull(SpriteRenderer[] arr)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != null) return false;
            }
            return true;
        }
    }
}
