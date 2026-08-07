namespace HASCore
{
    public static class GlobalKeyboardHook
    {
        private static KeyboardHook? _hook;
        private static Boolean _initialized = false;
        private static readonly HashSet<Keys> _pressedKeys = [];
        public static Boolean IsKeyDown(Keys key) => _pressedKeys.Contains(key);

        // Событие вызывается при нажатии/отпускании любой клавиши (изменении набора)
        public static event EventHandler<HashSet<Keys>>? KeysChanged;

        // Инициализация (вызывается один раз из любой формы)
        public static void Initialize()
        {
            if (_initialized) return;

            _hook = new KeyboardHook();
            _hook.KeyDown += OnKeyDown;
            _hook.KeyUp += OnKeyUp;
            _hook.Start();
            _initialized = true;

            // Автоматически останавливаем хук при выходе из приложения
            AppDomain.CurrentDomain.ProcessExit += (s, e) => _hook?.Dispose();
        }

        private static void OnKeyDown(Object? sender, Keys key)
        {
            if (_pressedKeys.Add(key)) // клавиша только что была нажата
            {
                // Уведомляем подписчиков об изменении набора
                KeysChanged?.Invoke(null, _pressedKeys);
            }
        }

        private static void OnKeyUp(Object? sender, Keys key)
        {
            if (_pressedKeys.Remove(key)) // клавиша только что была отпущена
            {
                KeysChanged?.Invoke(null, _pressedKeys);
            }
        }

        // Опционально: остановка хука (если нужно)
        public static void Shutdown()
        {
            if (_hook != null)
            {
                _hook.KeyDown -= OnKeyDown;
                _hook.KeyUp -= OnKeyUp;
                _hook.Dispose();
                _hook = null;
                _initialized = false;
                _pressedKeys.Clear();
            }
        }
    }
}