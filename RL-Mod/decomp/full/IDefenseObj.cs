public interface IDefenseObj
{
	float BaseKnockbackDefense { get; set; }

	float ActualKnockbackDefense { get; }

	float BaseStunDefense { get; set; }

	float ActualStunDefense { get; }
}
