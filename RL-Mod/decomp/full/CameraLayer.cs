public enum CameraLayer
{
	None = 0,
	Game = 1,
	Foreground_PERSP = 2,
	Foreground_ORTHO = 16,
	Background_ORTHO = 32,
	Background_Near_PERSP = 64,
	Background_Wall = 128,
	Background_Far_PERSP = 4,
	ForegroundLights = 8,
	Any = -1
}
