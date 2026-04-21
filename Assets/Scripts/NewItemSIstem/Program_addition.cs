// Añade en Program.cs, reemplaza el registro de IWeaponService si lo tenías:
AppContainer.Register<IEquipService>(() => new EquipService());

// En PlayerInputActions, en el mapa "Player", añade:
//   Use      → Button → Left Mouse Button
//   Reload   → Button → R  (si no lo tenías ya)
//   SwapItem → Value, Vector2 → Mouse/Scroll
