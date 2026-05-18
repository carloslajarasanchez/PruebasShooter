# PruebasShooter

## Introducción

**PruebasShooter** es un proyecto de videojuego **shooter de survival horror en primera persona** desarrollado en **Unity** con el motor **Unity 6000.3.12f1**. El juego está ambientado en un **asilo abandonado** donde el jugador debe explorar, sobrevivir y enfrentarse a criaturas hostiles.

### Características principales

- **Vista en primera persona** con cámara controlada por ratón, movimiento WASD, sprint, agacharse y saltar
- **Sistema de sigilo**: agacharse reduce el campo de visión de los enemigos, permitiendo evitarlos o acercarse sigilosamente
- **Combate con armas de fuego**: pistola y escopeta con munición limitada, recarga, retroceso visual y sistema de casquillos eyectados
- **Enemigos con IA avanzada**: tres tipos de enemigos (Ghoul, Esqueleto, Enemigo Gordo) con comportamientos únicos, máquina de estados (patrulla/persecución/ataque/investigación) y detección por cono de visión
- **Sistema de desmembramiento**: sistema completo que permite cortar extremidades o dejarlas colgando según el daño recibido, usando Animation Rigging. **Nota**: el sistema está implementado pero requiere adaptar los modelos de los enemigos para funcionar correctamente. El modo "colgando" (Dangle) necesita mejoras adicionales ya que no se comporta como se desea.
- **Inventario**: recoger items, equipar armas, usar consumibles (pociones de vida) y gestionar munición
- **Puertas con llaves**: encontrar llaves específicas para acceder a nuevas zonas del asilo
- **Sistema de guardado**: guardar partida en máquinas de save consumiendo cintas de tinta
- **Tutorial guiado**: sistema de tutoriales paso a paso que enseña movimiento, inventario y mecánicas básicas
- **Traducción completa**: soporte multiidioma (español por defecto) mediante archivos JSON
- **Audio inmersivo**: música ambiental de horror, sonidos de pasos según superficie, efectos de armas y sonidos de enemigos

### Arquitectura técnica

El proyecto utiliza una **arquitectura orientada a servicios** con un **Service Locator** (`AppContainer`) como núcleo. Cada sistema del juego está encapsulado en un servicio con su interfaz correspondiente, comunicándose entre sí mediante un **bus de eventos publish-subscribe**. La mayoría de los parámetros del juego son **configurables desde el editor de Unity** mediante ScriptableObjects e inspectores de componentes, sin necesidad de modificar código.

---

## Estructura del Proyecto

```
Assets/
├── Scripts/                          # CÓDIGO DEL JUEGO - Todos los sistemas
│   ├── Alert/                        # Sistema de alertas en pantalla
│   ├── Audio/                        # Sistema de audio y footstep controller
│   ├── Core/                         # Service locator y punto de entrada
│   ├── Enemigos/                     # Sistema de enemigos y combate
│   │   ├── ScriptsEnemigos/          # Implementaciones concretas de enemigos
│   ├── FootStep/                     # Sonidos de pasos por superficie
│   ├── Initialize/                   # Inicialización de escena
│   ├── Input/                        # Gestión de input (Unity Input System)
│   ├── Inventory/                    # Sistema de inventario e items
│   │   ├── Items/                    # Implementaciones de items concretos
│   │   └── Weapons/                  # Sistema de armas
│   │       └── Weapons/              # Armas concretas (pistola, escopeta)
│   ├── NewItemSIstem/                # Sistema de equipamiento
│   ├── Pause/                        # Sistema de pausa
│   ├── Player/                       # Movimiento del jugador y cámara
│   ├── Pusheables/                   # Objetos empujables
│   │   └── Objects/                  # Puertas y objetos específicos
│   ├── Save/                         # Interfaces de guardado
│   ├── Services/                     # SERVICIOS (arquitectura orientada a servicios)
│   │   ├── Alerts/                   # Servicio de alertas
│   │   ├── Audio/                    # Servicio de audio
│   │   ├── Configuration/            # Servicio de configuración
│   │   ├── Events/                   # Servicio de eventos publish-subscribe
│   │   │   └── EventS/               # Clases de eventos concretos (23 eventos)
│   │   ├── GameState/                # Servicio de estado del juego (flags/triggers)
│   │   ├── Inventory/                # Servicio de inventario
│   │   ├── Log/                      # Servicio de logging
│   │   ├── Pause/                    # Servicio de pausa
│   │   ├── Player/                   # Servicio de jugador (vidas)
│   │   ├── Pool/                     # Servicio de object pooling
│   │   ├── Save/                     # Servicio de guardado/carga
│   │   │   └── EntryS/               # Tipos de entradas de guardado
│   │   ├── Scene/                    # Servicio de gestión de escenas
│   │   ├── Translation/              # Servicio de traducción
│   │   ├── Weapon/                   # Servicio de armas
│   │   └── Zones/                    # Servicio de zonas
│   ├── TestTutorial/                 # Tutorial legacy
│   ├── Translations/                 # Componentes de traducción en UI
│   ├── UI/                           # Sistemas de interfaz de usuario
│   │   ├── Effects/                  # Efectos de botones UI
│   │   ├── Game/                     # UI del juego (HUD, inventario, game over)
│   │   └── MainMenu/                 # UI del menú principal
│   ├── WorkflowSystem/               # Sistema de tutoriales por pasos
│   │   └── Steps/                    # Pasos concretos del tutorial
│   └── Zones/                        # Componentes de zonas en escena
│
├── InputAction/                      # Actions del Unity Input System (autogenerado)
├── Resources/                        # Archivos cargados en runtime (i18n, etc.)
├── ScriptableObject/                 # ScriptableObjects del juego (datos de items, armas, audio)
│
├── Scenes/                           # Escenas del proyecto
├── Prefabs/                          # Prefabs del juego
├── Materials/                        # Materiales
├── PhysicsMaterials/                 # Physics materials para detección de superficies
├── Sounds/                           # Archivos de audio
├── AudioUI/                          # Audio para UI
├── Sprites/                          # Sprites/UI assets
├── Fonts/                            # Fuentes del juego
├── Fuentes/                          # Fuentes adicionales
├── Imagenes/                         # Imágenes varias
│
├── ModelosEnemigos/                  # Modelos 3D de enemigos
├── Mutant1/                          # Modelo de mutante
├── _GhoulZombie/                     # Modelo de ghoul
├── _Recovery/                        # Modelo de recovery
├── Low_Poly_flashlight_v01/          # Modelo de linterna
├── kOsmaragd/                        # Asset de terceros
├── Abandoned_Asylum/                 # Assets del escenario asilo
├── Horror Ambient Album - 060319/    # Música ambiental
├── PS1 Cans Asset Pack/              # Asset pack de latas
├── PSXAmmoBoxes/                     # Asset pack de cajas de munición
├── PSXMiscGuns/                      # Asset pack de armas
│
├── RenderTexture/                    # Render textures para cámaras
├── Settings/                         # Configuraciones de Unity (URP, input, etc.)
├── Standard Assets/                  # Assets estándar de Unity
├── TextMesh Pro/                     # Plugin de TextMeshPro
├── TypeWriter/                       # Efecto typewriter para texto
├── TutorialInfo/                     # Info de tutoriales de Unity
└── inkRibbon/                        # Asset de cinta de tinta
```

### Resumen de carpetas principales

| Carpeta | Contenido |
|---|---|
| **Scripts/** | Todo el código C# del juego, organizado por sistemas |
| **Scripts/Services/** | Servicios reutilizables con interfaces (arquitectura orientada a servicios) |
| **Scripts/UI/** | Toda la interfaz de usuario dividida en HUD, menú principal y efectos |
| **Resources/** | Archivos JSON de traducción (i18n) cargados en runtime |
| **ScriptableObject/** | Datos configurables desde el editor (items, armas, sonidos) |
| **Scenes/** | Escenas del juego (menú, nivel principal, etc.) |
| **Prefabs/** | Prefabs reutilizables del juego |
| **ModelosEnemigos/, Mutant1/, etc.** | Modelos 3D y assets de terceros (no requieren modificación) |

---

## Configuración desde el Editor de Unity

> **Nota:** La gran mayoría de parámetros del juego son modificables directamente desde el editor de Unity sin necesidad de tocar código.

### ScriptableObjects (Assets configurables)

| ScriptableObject | Qué configura |
|---|---|
| **ItemData** | Nombre, descripción, icono sprite, modelo 3D de cada item |
| **WeaponData** | Munición máxima, fire rate, tiempo de recarga, rango, daño, modo auto/semi, pellet count, spread angle, tipo de arma, prefab de casquillo |
| **SoundLibrary** | Base de datos completa de sonidos mapeados por tipo |
| **SoundData** | AudioClip(s), volumen, pitch, variación de pitch, loop |
| **FootstepSurface** | Mapeo de PhysicsMaterials a tipos de sonido de pasos |
| **DoorWorkflowConfig** | Configuración de tutoriales específicos de puertas |

### Componentes en Inspector (MonoBehaviours)

**Player:**
- `PlayerController`: velocidad de movimiento, fuerza de salto, altura de agachado, overhead check distance
- `CameraController`: sensibilidad mouse, smoothing, límites verticales, intensidad de tilt, intensidad de step-bob
- `ObjectEquipedAnimation`: intensidad de mouse sway, movement sway, walking bob, recoil

**Enemigos:**
- `BaseEnemy`: waypoints de patrulla, rango y ángulo de visión, velocidad de persecución, daño de ataque, cooldown de ataque, vida total, configuración de limbs para desmembramiento
- `HitReactionRig`: fuerza de spring, damping, modo de desmembramiento (Sever/Dangle/None)
- `EnemyHeadAim`: modo de mira (Tracking/Searching/Idle)

**Armas:**
- `Weapon`: todos los stats vienen de `WeaponData` (ScriptableObject asignable en inspector)

**Audio:**
- `AudioService`: número inicial de AudioSources en el pool
- `WorldAudioSource`: SoundType a reproducir, SoundLibrary de referencia

**UI:**
- `UIInventory`: referencia al grid layout
- `UIItemDetail`: cámara dedicada para preview 3D, layer de preview
- `ButtonEffect`: colores de hover, escala, duración de shake, sonidos
- `SplashController`: duración antes de cambiar de escena
- `MenuCameraMovement`: velocidad de parallax, amplitud de oscilación

**Puertas:**
- `DoorController`: KeyEnum requerida, estado inicial, sonido de puerta
- `PusheableObject`: toggle CanBePushed, masa, drag

**Zonas:**
- `Zone`: ID de zona
- `ZoneTrigger`: zona asociada al trigger

**Traducción:**
- `TranslatableItem` / `TranslatableItemTextMesh`: key de traducción asignable en inspector

**Tutorial:**
- `Workflow`: lista de steps configurables en inspector
- Cada Step: nombre, descripción, mensaje de alerta

**Servicios (en GameObjects de escena):**
- `SaveMachine`: SaveId único para guardado
- `InitializeGame`: escena de inicio, configuración de tutorial

---

## Sistemas del Proyecto

### 1. Sistema Core / Inyección de Dependencias

**Archivos:**
- `Scripts/Core/AppContainer.cs`
- `Scripts/Core/Program.cs`

**Cómo funciona:**

El proyecto usa un patrón **Service Locator estático** como núcleo de arquitectura. `AppContainer` es una clase estática que almacina fábricas (`Func<object>`) para cada servicio. Cuando se llama `Get<T>()`, el contenedor crea la instancia de forma **lazy** (solo cuando se necesita por primera vez). Esto evita problemas de orden de inicialización en Unity.

`Program.cs` es el punto de entrada de la aplicación. Usa `[RuntimeInitializeOnLoadMethod]` para ejecutarse automáticamente antes de que cargue cualquier escena. Aquí se **registran TODOS los servicios** en el contenedor: Scene, Log, Configuration, Events, Translation, Input, Inventory, Player, Equip, Zone, Pool, GameState, Save, Alert, Pause y Audio. Cada MonoBehaviour del proyecto accede a los servicios mediante `AppContainer.Get<IService>()`.

---

### 2. Sistema de Movimiento del Jugador y Cámara

**Archivos:**
- `Scripts/Player/PlayerController.cs`
- `Scripts/Player/CameraController.cs`
- `Scripts/Player/PlayerPush.cs`
- `Scripts/Player/ObjectEquipedAnimation.cs`

**Cómo funciona:**

`PlayerController` usa `CharacterController` de Unity para movimiento WASD. Maneja:
- Movimiento relativo a la cámara (transform.forward/right)
- Sistema de agacharse/levantarse con **detección de espacio superior** (raycast para evitar levantarse bajo techos bajos)
- Salto con gravedad personalizada
- Restauración de posición al cargar partida
- Publica eventos `OnPlayerCrouch` y `OnPlayerStand` para que otros sistemas reaccionen

`CameraController` controla la cámara first-person:
- Mouse look con **suavizado configurable** y límites verticales de rotación
- Transición suave de altura al agacharse/levantarse
- **Inclinación lateral** al girar (tilt en el eje Z proporcional a velocidad de giro)
- **Step-bob**: movimiento vertical sinusoidal de la cabeza al caminar, usado también por el sistema de footstep audio

`PlayerPush` detecta colisiones del CharacterController y aplica fuerza horizontal a objetos que implementen `IPusheable` (como puertas).

`ObjectEquipedAnimation` aplica animaciones procedurales al arma/item equipado:
- **Mouse sway**: el arma sigue ligeramente la cámara con delay
- **Movement sway**: oscilación al moverse
- **Walking bob**: balanceo al caminar sincronizado con step-bob
- **Shooting recoil**: animación de retroceso al disparar

---

### 3. Sistema de Input

**Archivos:**
- `Scripts/Input/PlayerInputManager.cs`
- `Scripts/Input/IPlayerInput.cs`
- `Scripts/Input/ControlMap.cs`
- `InputAction/PlayerInputActions.cs` (autogenerado)

**Cómo funciona:**

Usa el **Unity Input System** con dos action maps:

**Action Map "Player":** Move, Camera, Interact, Run, Crouch, Jump, Inventory, Use, Reload, SwapItem, SwitchLight, Pause

**Action Map "UI":** Navigation, Submit, Cancel, Inventory, Pause

`PlayerInputManager` gestiona la transición entre ambos maps:
- Map `Player` → cursor bloqueado, input de movimiento activo
- Map `UI` → cursor visible, input de UI activo
- Cambia automáticamente al abrir/cerrar inventario o pausa

---

### 4. Sistema de Enemigos y Combate

**Archivos:**
- `Scripts/Enemigos/BaseEnemy.cs`
- `Scripts/Enemigos/EnemyStateMachine.cs`
- `Scripts/Enemigos/Hitbox.cs`
- `Scripts/Enemigos/AttackHitbox.cs`
- `Scripts/Enemigos/HitReactionRig.cs`
- `Scripts/Enemigos/EnemyHeadAim.cs`
- `Scripts/Enemigos/DismembermentMode.cs`
- `Scripts/Enemigos/LimbData.cs`
- `Scripts/Enemigos/EnemyState.cs`
- `Scripts/Enemigos/ScriptsEnemigos/Ghoul.cs`
- `Scripts/Enemigos/ScriptsEnemigos/EsqueletoEnemigo.cs`
- `Scripts/Enemigos/ScriptsEnemigos/EnemigoGordo.cs`

**Cómo funciona:**

`BaseEnemy` es el corazón del sistema (543 líneas). Implementa `ISavable<EnemyState>` e `IPusheable`.

**Máquina de estados con NavMeshAgent:**
- **Idle**: Patrulla entre waypoints
- **Chasing**: Persigue al jugador cuando lo detecta
- **Attacking**: Ataca al jugador en rango (con cooldown y animation events)
- **Investigating**: Investiga la última posición conocida del jugador
- **Dead**: Activa ragdoll y reproduce animación de muerte

**Sistema de detección:**
- **Cono de visión**: angle y range configurables, con line-of-sight check (raycast)
- **Detección trasera**: detecta al jugador si está detrás a corta distancia
- **Sigilo**: si el jugador está agachado, el enemigo reduce su visión (50% range, 60% angle)

**Sistema de combate:**
- `Hitbox` en cada parte del cuerpo recibe daño con multiplicador y lo forward a `BaseEnemy.TakeDamage()`
- `AttackHitbox` es un trigger collider en la animación de ataque. Cuando `CanDealDamage = true`, daña al jugador vía `IPlayer.RestLives()`

**Sistema de reacciones a impactos (`HitReactionRig`):**
Usa **Unity Animation Rigging** para crear constraints en runtime:
- OverrideTransform para cabeza/torso
- TwoBoneIK para brazos/piernas
- Al recibir impacto, aplica desplazamiento con **spring-damper** para simular el golpe

**Sistema de desmembramiento:**
- `LimbData` define por cada hueso: vida actual, vida máxima, fuerza instantánea para cortar, si es central (si se destruye = enemigo muere)
- Dos modos (`DismembermentMode`): **Sever** (la extremidad sale volando) o **Dangle** (cuelga suelta)

**Tipos de enemigo:**
- **Ghoul**: Cono de visión + detección trasera. Sigilo con crouch reduce visión.
- **Esqueleto**: Usa zona de detección tipo box. Si el jugador lo mira, se congela (se queda idle). Si no, persigue.
- **EnemigoGordo**: Igual que Ghoul.

---

### 5. Sistema de Inventario

**Archivos:**
- `Scripts/Inventory/InventoryController.cs`
- `Scripts/Inventory/Item.cs`
- `Scripts/Inventory/ItemData.cs`
- `Scripts/Inventory/ItemState.cs`
- `Scripts/Inventory/ICatchable.cs`
- `Scripts/Inventory/IInventory.cs`
- **Items:** `Key.cs`, `HealthPotion.cs`, `SaveTape.cs`, `SwitchLight.cs`
- **Munición:** `BulletsBase.cs`, `PistoleBullets.cs`, `ShotgunBullets.cs`, `IBullet.cs`
- **Enums:** `KeyEnum.cs`

**Cómo funciona:**

`InventoryController` se adjunta al player. Hace **raycast desde el centro de la pantalla** para detectar:
- Items que implementen `ICatchable` → muestra icono de interacción
- `SaveMachine` → permite guardar partida
- Publica `OnCatchableDetected`/`OnCatchableLost` para la UI
- En la primera recogida de item, dispara el **tutorial de inventario**

`Item` es la clase base abstracta. Implementa `ICatchable` e `ISavable<ItemState>`. Define:
- `Catch()`: recogida del item (añade al inventario, desactiva GameObject)
- `Equip()`: equipar el item
- `Use()`: usar el item
- Save/Restore del estado

`ItemData` es un ScriptableObject que define propiedades visuales del item: nombre, descripción, icono sprite, modelo 3D prefab.

`ItemState` guarda: `isInInventory`, `isConsumed`, `currentAmmo`.

**Items concretos:**
- **Key**: Tiene un `KeyEnum` (StoreKey, DiningRoomKey, BasementKey, BoilersKey). No se equipa, se usa para abrir puertas.
- **HealthPotion**: Consumible que restaura 20 HP. `IsReusable = false`.
- **SaveTape**: Consumible usado en SaveMachines. Se consume al guardar.
- **SwitchLight**: Toggle de linterna con tecla G.

**Munición:**
- `BulletsBase` es abstracta, implementa `IBullet` con `BulletAmount` y `Type`.
- `PistoleBullets`: 30 balas por defecto, tipo `Pistole`.
- `ShotgunBullets`: 20 balas por defecto, tipo `Shotgun`.

---

### 6. Sistema de Armas

**Archivos:**
- `Scripts/Inventory/Weapons/Weapon.cs`
- `Scripts/Inventory/Weapons/WeaponData.cs`
- `Scripts/Inventory/Weapons/IWeapon.cs`
- `Scripts/Inventory/Weapons/SkullCap.cs`
- `Scripts/Inventory/Weapons/Weapons/Pistole.cs`
- `Scripts/Inventory/Weapons/Weapons/ShootGun.cs`
- `Scripts/Inventory/Weapons/WeaponTypeEnum.cs`

**Cómo funciona:**

`Weapon` extiende `Item` e implementa `IEquippable`. Es el sistema más complejo del inventario:

**Gestión de munición:**
- `CurrentAmmo` / `MaxAmmo` del cargador
- `ReserveAmmo` del inventario (se consulta vía `IBullet`)
- Auto-recarga cuando se vacía el cargador y hay munición de reserva

**Disparo:**
- Raycast desde la cámara con **spread configurable**
- Soporte **pellet count** para escopeta (múltiples raycasts)
- **Laser sight** visible
- **VFX** de impacto
- **Audio** de disparo
- **Fire rate** configurable
- **Auto vs semi-auto**: las armas automáticas disparan mientras se mantenga el botón

**Recarga:**
- Animación con **ReloadTime** configurable
- Consume munición del inventario (`IBullet`)
- Publica eventos `OnWeaponReloading`, `OnWeaponReloaded`, `OnAmmoChanged`

**Casquillos:**
- `SkullCap` es el VFX de casquillo eyectado
- Se instancia desde el **PoolService** (no se destruyen)
- Al activarse, aplica torque aleatorio y fuerza hacia arriba
- Vuelve al pool tras su lifetime

`WeaponData` es ScriptableObject con: MaxAmmo, FireRate, ReloadTime, Range, Damage, IsAutomatic, HitForce, PelletCount, SpreadAngle, WeaponType, CasingPrefab.

---

### 7. Sistema de Equipamiento (NewItemSIstem)

**Archivos:**
- `Scripts/NewItemSIstem/EquipService.cs`
- `Scripts/NewItemSIstem/EquipController.cs`
- `Scripts/NewItemSIstem/IEquippable.cs`
- `Scripts/NewItemSIstem/IEquipService.cs`
- `Scripts/NewItemSIstem/EquipEvents.cs`

**Cómo funciona:**

`EquipService` gestiona qué item tiene el jugador en la mano:
- Mueve items entre transform `ItemStorage` (oculto) y `Hand` (visible)
- Cambia layers de los items (Hand vs Default) para que solo se vean en la mano
- **Consumibles**: al usarlos (`OnPrimaryAction`), se consumen automáticamente y se vuelve al item anterior
- `CurrentItem` / `PreviousItem` para swap con scroll wheel

`EquipController` se adjunta al player y mapea inputs:
- **Use** (click izquierdo): `OnPrimaryAction()` del item equipado
- **Reload** (R): recarga del arma
- **SwapItem** (scroll wheel): cambia entre item actual y anterior
- Diferencia entre armas **single-shot** (un disparo por click) y **automatic** (disparo continuo mientras se mantenga)

---

### 8. Sistema de Audio

**Archivos:**
- `Scripts/Audio/AudioService.cs`
- `Scripts/Audio/SoundLibrary.cs`
- `Scripts/Audio/SoundData.cs`
- `Scripts/Audio/SoundType.cs`
- `Scripts/Audio/ISoundLibrary.cs`
- `Scripts/Audio/WorldAudioSource.cs`
- `Scripts/Audio/InitializerMusicScene.cs`
- `Scripts/FootStep/FootstepController.cs`
- `Scripts/FootStep/FootstepSurface.cs`

**Cómo funciona:**

`AudioService` es un MonoBehaviour con **pooling de AudioSources** (9 iniciales, auto-expandible):
- Música de fondo con crossfade
- Sonidos one-shot con pitch aleatorio configurable
- Sonidos en loop
- Control de volumen master
- Play/Stop/Pause/Resume por fuente

`SoundLibrary` es un ScriptableObject que actúa como base de datos de sonidos. Mapea `SoundType` → `SoundData` mediante diccionario.

`SoundData` define: AudioClip array (para variación), volumen, pitch, pitchVariation, loop.

`SoundType` tiene 30+ tipos: música, UI, footsteps, armas, enemigos, ambiental, puertas, zonas.

`FootstepController` detecta el material del suelo vía raycast. Escucha la onda sinusoidal del step-bob de la cámara; cuando cruza umbral, reproduce el sonido de pie correspondiente al material de superficie.

`FootstepSurface` es ScriptableObject que mapea PhysicsMaterials a SoundTypes con fallback.

---

### 9. Sistema de Guardado/Carga

**Archivos:**
- `Scripts/Save/SaveMachine.cs`
- `Scripts/Save/ISavable.cs`
- `Scripts/Services/Save/SaveService.cs`
- `Scripts/Services/Save/SaveData.cs`
- `Scripts/Services/Save/EntryS/DoorSaveEntry.cs`
- `Scripts/Services/Save/EntryS/EnemySaveEntry.cs`
- `Scripts/Services/Save/EntryS/FlagEntry.cs`
- `Scripts/Services/Save/EntryS/ItemSaveEntry.cs`
- `Scripts/Services/Save/EntryS/PlayerSaveEntry.cs`

**Cómo funciona:**

El guardado se hace en **JSON** en `Application.persistentDataPath/save.json`.

`SaveMachine` es un objeto de mundo con el que el jugador interactúa. Abre `UIConfirmationSaveMenu`. Verifica que el jugador tenga un `SaveTape` en el inventario, lo consume, y llama a `SaveService.Save()`.

`SaveService.Save()` persiste:
- **Flags**: estado del mundo (puertas abiertas, eventos completados)
- **Triggers**: eventos de un solo uso
- **Items**: estado de cada item (en inventario, consumido, munición)
- **Doors**: estado abierto/cerrado
- **Enemies**: vivo/muerto
- **Player**: posición X/Y/Z, rotación Y del player, rotación X de cámara, vidas
- **Equipados**: ID del item actual y anterior

`SaveService.Load()` restaura:
- Busca todos los objetos `ISavable<T>` en la escena por su `SaveId`
- Restaura estado de puertas, enemigos, items, player
- Si no hay save file, no hace nada

`ISavable<T>` es interfaz genérica con `SaveId` (string único) y `RestoreState(T)`.

---

### 10. Sistema de Estado del Juego (Flags/Triggers)

**Archivos:**
- `Scripts/Services/GameState/GameState.cs`
- `Scripts/Services/GameState/IGameState.cs`

**Cómo funciona:**

Dos diccionarios de booleanos:
- **Flags**: persistentes, no se resetean. Representan estado permanente del mundo (ej: "tutorial completado", "jefe derrotado").
- **Triggers**: se resetean automáticamente tras ser leídos. Eventos de un solo uso (ej: "jugador entró en zona X").

Publica `OnFlagChangedEvent` y `OnTriggerChangedEvent` cuando cambian, permitiendo que otros sistemas reaccionen.

---

### 11. Sistema de Eventos

**Archivos:**
- `Scripts/Services/Events/EventService.cs`
- `Scripts/Services/Events/IEventService.cs`
- `Scripts/Services/Events/OwnEventBase.cs`
- `Scripts/Services/Events/EventS/` (23 clases de eventos)

**Cómo funciona:**

`EventService` es un **bus publish-subscribe**. Mapea tipos de evento a listas de handlers (`Action<T>`).

- `Publish<T>(T event)`: notifica a todos los suscriptores de ese tipo
- `Subscribe<T>(Action<T>)`: registra un handler
- `Unsubscribe<T>(Action<T>)`: elimina un handler
- Convierte a array durante iteración para evitar problemas de modificación concurrente

**Los 23 eventos:**
`OnAlertMessageReceived`, `OnAmmoChanged`, `OnCatchableDetected`, `OnCatchableLost`, `OnDoorLocked`, `OnEnemyKilled`, `OnFirstEquipItem`, `OnFirstSelectedItem`, `OnFlagChangedEvent`, `OnGameOver`, `OnGamePaused`, `OnGameResumed`, `OnInventoryChanged`, `OnLanguageChanged`, `OnLivesChanged`, `OnPlayerCrouch`, `OnPlayerStand`, `OnPlayerShoot`, `OnTriggerChangedEvent`, `OnWeaponChanged`, `OnWeaponFired`, `OnWeaponReloaded`, `OnWeaponReloading`

---

### 12. Sistema de Pausa

**Archivos:**
- `Scripts/Pause/PauseController.cs`
- `Scripts/Pause/UIPauseView.cs`
- `Scripts/Services/Pause/PauseService.cs`
- `Scripts/Services/Pause/IPauseService.cs`

**Cómo funciona:**

`PauseController` escucha input Pause (Escape en ambos maps Player y UI). Llama a `IPauseService.Toggle()`.

`PauseService` gestiona el estado: `Toggle()`, `Resume()`, `IsPaused`, `IsPauseBlocked` (para evitar pausa en cinemáticas).

`UIPauseView` muestra/oculta el panel de pausa. Cambia control maps (Player↔UI) y estado del cursor. Botones de Continue y Quit.

---

### 13. Sistema de UI

**Archivos HUD:**
- `Scripts/UI/Game/UIInventory.cs`
- `Scripts/UI/Game/UIItem.cs`
- `Scripts/UI/Game/UIItemDetail.cs`
- `Scripts/UI/Game/WeaponAmmoUI.cs`
- `Scripts/UI/Game/ItemDetector.cs`
- `Scripts/UI/Game/UIGameOverView.cs`
- `Scripts/UI/Game/UIConfirmationSaveMenu.cs`
- `Scripts/UI/Game/DeactivateInventory.cs`
- `Scripts/UI/Game/GoToMainMenu.cs`

**Archivos Menú Principal:**
- `Scripts/UI/MainMenu/LoadSceneButton.cs`
- `Scripts/UI/MainMenu/MenuCameraMovement.cs`
- `Scripts/UI/MainMenu/SplashController.cs`
- `Scripts/UI/MainMenu/QuitButton.cs`
- `Scripts/UI/MainMenu/BackToMainMenu.cs`

**Archivos de Efectos:**
- `Scripts/UI/VolumeSlider.cs`
- `Scripts/UI/Effects/ButtonEffect.cs`

**Cómo funciona:**

**HUD:**
- `UIInventory`: Panel que abre con Tab. Muestra grid de items del `InventoryService`. Soporta selección y panel de detalle. Se cierra al pausar.
- `UIItem`: Slot individual. Muestra nombre (traducible) e icono. Al click, muestra detalle y publica `OnFirstSelectedItem` para tutorial.
- `UIItemDetail`: Panel derecho con nombre, descripción, modelo 3D rotatorio (cámara dedicada con layer propio), y botón Equip.
- `WeaponAmmoUI`: HUD de munición. Muestra actual/reserva. Color cambia según nivel (blanco > naranja > rojo). Suscrito a eventos de equip/unequip/ammo/inventario.
- `ItemDetector`: Icono de crosshair. Cambia sprite al mirar items recogibles (suscrito a OnCatchableDetected/Lost).
- `UIGameOverView`: Pantalla de muerte. Animación (player rota 90°), luego panel con Restart/Quit. Limpia servicios al reiniciar.
- `UIConfirmationSaveMenu`: Diálogo de guardado. Verifica SaveTape, lo consume, muestra texto traducido.
- `DeactivateInventory`: Toggle del GameObject según estado del inventario.
- `GoToMainMenu`: Limpia servicios y carga escena "02_MainMenu".

**Menú Principal:**
- `LoadSceneButton`: Carga escena especificada vía ISceneService.
- `MenuCameraMovement`: Movimiento parallax de cámara según posición del ratón + oscilación idle.
- `SplashController`: Avanza automáticamente a siguiente escena tras duración configurable.
- `QuitButton`: Sale del juego.
- `BackToMainMenu`: Carga escena anterior.

**Efectos:**
- `ButtonEffect`: Hover glow, hover scale, click shake, hover/click sounds.
- `VolumeSlider`: Controla `IAudioService.MasterVolume`.

---

### 14. Sistema de Traducción (i18n)

**Archivos:**
- `Scripts/Services/Translation/JsonTranslationService.cs`
- `Scripts/Services/Translation/ITranslationService.cs`
- `Scripts/Translations/ChangeLanguage.cs`
- `Scripts/Translations/TranslatableItem.cs`
- `Scripts/Translations/TranslatableItemTextMesh.cs`
- `Scripts/Translations/TranslateItem.cs`
- `Scripts/Translations/TranslationsDTO.cs`

**Cómo funciona:**

`JsonTranslationService` carga archivos JSON desde `Resources/i18n/`. Cada archivo es un idioma. Proporciona `Get(key)` y `ChangeLanguage()`.

`TranslatableItem` se adjunta a TextMeshProUGUI. Se suscribe a `OnLanguageChanged` y actualiza el texto usando una key del servicio.

`TranslatableItemTextMesh` igual pero para TextMeshPro 3D (world-space).

`ChangeLanguage` es un componente de botón que cambia el idioma global.

---

### 15. Sistema de Tutorial / Workflow

**Archivos:**
- `Scripts/WorkflowSystem/Workflow.cs`
- `Scripts/WorkflowSystem/IStep.cs`
- `Scripts/WorkflowSystem/Steps/WalkStep.cs`
- `Scripts/WorkflowSystem/Steps/MoveCameraStep.cs`
- `Scripts/WorkflowSystem/Steps/CrouchStep.cs`
- `Scripts/WorkflowSystem/Steps/RunStep.cs`
- `Scripts/WorkflowSystem/Steps/InteractItemStep.cs`
- `Scripts/WorkflowSystem/Steps/OpenInventoryStep.cs`
- `Scripts/WorkflowSystem/Steps/SelectFirstItemStep.cs`
- `Scripts/WorkflowSystem/Steps/EquipItemStep.cs`
- `Scripts/WorkflowSystem/Steps/StoreKeyStep.cs`
- `Scripts/TestTutorial/Tutorial.cs`
- `Scripts/Pusheables/Objects/DoorWorkflowConfig.cs`

**Cómo funciona:**

`Workflow` orquesta una **secuencia de ISteps**. Activa pasos uno a uno con delays de 2 segundos entre ellos. Muestra alerts para cada paso. Al completar todos, muestra alerta de felicitación e invoca callback `OnComplete`.

Cada `IStep` tiene: Name, Description, IsCompleted, Activate(), Deactivate(), evento OnCompleted.

**Pasos del tutorial:**
- `WalkStep`: Espera a que el jugador pulse teclas de movimiento.
- `MoveCameraStep`: Espera a que mueva la cámara.
- `CrouchStep`: Espera a que se agache.
- `RunStep`: Espera a que corra (Shift).
- `InteractItemStep`: Espera a que interactúe/recoga un item.
- `OpenInventoryStep`: Espera a que abra inventario (Tab).
- `SelectFirstItemStep`: Espera evento `OnFirstSelectedItem`.
- `EquipItemStep`: Espera evento `OnFirstEquipItem`.
- `StoreKeyStep`: Espera evento `OnDoorLocked` (enseña sobre llaves).

`DoorWorkflowConfig` es ScriptableObject que configura tutoriales específicos de puertas. Crea ISteps dinámicamente por nombre de tipo.

---

### 16. Sistema de Zonas

**Archivos:**
- `Scripts/Services/Zones/ZoneService.cs`
- `Scripts/Services/Zones/IZoneService.cs`
- `Scripts/Zones/Zone.cs`
- `Scripts/Zones/ZoneTrigger.cs`

**Cómo funciona:**

`ZoneService` gestiona zonas con **reference counting**:
- `RegisterZone(id)`: registra zona
- `EnterZone(id)`: incrementa refCount, activa el GameObject de la zona
- `ExitZone(id)`: decrementa refCount, desactiva cuando llega a 0
- Útil para activar/desactivar audio ambiental por zona

`Zone` registra su ID con ZoneService en Awake.

`ZoneTrigger` es un trigger collider que llama EnterZone/ExitZone cuando el player entra/sale.

---

### 17. Sistema de Objetos Empujables y Puertas

**Archivos:**
- `Scripts/Pusheables/PusheableObject.cs`
- `Scripts/Pusheables/IPusheable.cs`
- `Scripts/Pusheables/Objects/DoorController.cs`
- `Scripts/Pusheables/Objects/DoorState.cs`
- `Scripts/Pusheables/Objects/DoorWorkflowConfig.cs`

**Cómo funciona:**

`IPusheable` define `Push(Vector3 force)`.

`PusheableObject` es base para objetos empujables. Tiene toggle `CanBePushed`. Aplica fuerza al Rigidbody.

`DoorController` es una puerta con **requisito de llave**:
- Al entrar en trigger, comprueba inventario del player para `KeyEnum` matching
- Si tiene llave → abre/cierra, reproduce sonido
- Si no tiene llave → puede iniciar tutorial específico de puerta
- Guarda/restaura estado abierto/cerrado
- Implementa `ISavable<DoorState>`

---

### 18. Sistema de Alertas

**Archivos:**
- `Scripts/Services/Alerts/IAlertService.cs`
- `Scripts/Alert/UIAlertView.cs`

**Cómo funciona:**

`IAlertService` define `Show(description, title)`.

`UIAlertView` muestra mensajes con TextMeshPro. Se suscribe a `OnAlertMessageReceived`. Soporta título/descripción traducidos. Se cierra con mensaje vacío.

---

### 19. Sistema de Inicialización

**Archivos:**
- `Scripts/Initialize/InitializeGame.cs`

**Cómo funciona:**

Inicializador de escena. En Start:
1. Habilita input del player
2. Inicializa zonas
3. Carga partida guardada
4. Si vidas <= 0, resetea player
5. Reproduce música del asilo
6. Inicia tutorial de movimiento si no está completado

---

### 20. Servicios Auxiliares

**Archivos:**
- `Scripts/Services/Inventory/InventoryService.cs` - Lista de items. AddItem, RemoveItem, GetItem<T> con predicate, Clear. Publica OnInventoryChanged.
- `Scripts/Services/Player/Player.cs` - Gestión de vidas (default 100). RestLives (cooldown 2s), AddLives, SetLives, ResetPlayer. Publica OnGameOver cuando vidas <= 0.
- `Scripts/Services/Pool/PoolService.cs` - Object pooling. Crea contenedor DontDestroyOnLoad con colas por prefab. Get (dequeue o instantiate), Return (desactiva y enqueue).
- `Scripts/Services/Scene/SceneService.cs` - LoadScene, LoadNextScene, LoadPreviousScene, QuitGame.
- `Scripts/Services/Configuration/ConfigurationService.cs` - Store key-value. Default language = "es".
- `Scripts/Services/Log/LogService.cs` - Logging para debug.
- `Scripts/Services/Weapon/WeaponService.cs` - Servicio de armas (registrado pero no usado directamente por MonoBehaviours).
