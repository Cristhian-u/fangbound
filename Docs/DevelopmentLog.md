# Registro de desarrollo

## Tarea 01: Preparación y prototipo inicial

### Estado inicial encontrado

- Unity Editor del proyecto: `6000.5.5f1`.
- URP está instalado (`com.unity.render-pipelines.universal` `17.6.0`) y el perfil de calidad activo referencia un asset URP. El Renderer configurado es `Renderer2D`.
- El Input System está instalado (`com.unity.inputsystem` `1.19.0`), `activeInputHandler` está en modo Input System y existe `Assets/Settings/InputSystem_Actions.inputactions` con un mapa `Player`.
- La gravedad 2D existente es `(0, -9.81)`; se mantienen los valores de iteraciones y demás ajustes del proyecto.
- Existe `Assets/Scenes/SampleScene.unity`, con cámara ortográfica predeterminada. Se conserva sin cambios.
- No se encontraron scripts C# propios ni escena de pruebas previa.
- La estructura propia existente usa carpetas numeradas vacías (`00_Scripts`, `01_Data`, `03_Prefabs`, `04_Scenes`, `05_Art`, `06_Audio`, `07_Settings`). Se reutiliza; no se reorganiza.
- Antes de esta tarea ya aparecían modificaciones locales en `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`, `ProjectSettings/EditorBuildSettings.asset`, `ProjectSettings/ProjectSettings.asset`, `ProjectSettings/ShaderGraphSettings.asset` y contenido sin seguimiento en `Docs/`. Esos cambios se conservaron. `Docs/GDD.md` se leyó y no se modificó.

### Archivos creados

- `AGENTS.md`: reglas persistentes para el trabajo futuro.
- `Assets/_Project/00_Scripts/Editor/MovementTestSceneGenerator.cs`: comando de Editor que genera la escena de prueba con objetos y componentes Unity.
- `Assets/_Project/05_Art/Placeholder/MovementTestSquare.asset`: sprite cuadrado blanco creado localmente por el generador para los placeholders.
- `Assets/_Project/Scenes/Test/MovementTest.unity`: escena de pruebas generada con Unity Editor.
- `Docs/DevelopmentLog.md`: este registro.
- `Docs/Region1Plan.md`: propuesta de conexiones para 16 salas; no son salas implementadas.
- Unity creó los `.meta` asociados a carpetas y assets, y `ProjectSettings/SceneTemplateSettings.json` durante la ejecución del Editor.

### Configuración realizada

- No se instalaron paquetes ni se modificó la configuración global de proyecto.
- El generador está preparado para crear cámara ortográfica, suelo dividido para dejar un hueco, tres plataformas a distintas alturas, pared, punto de aparición, salamandra provisional oscura con indicador de dirección y colisionadores 2D.
- La salamandra usa Rigidbody2D y CapsuleCollider2D, sin controlador ni lógica de movimiento.
- No se añadió MovementTest a Build Settings para evitar alterar la lista de escenas que ya tenía cambios locales.

### Problemas detectados y verificación

- La carpeta `Assets/_Project/Scenes/Test` no existía y se crea únicamente para la escena solicitada.
- La escena se genera mediante la API de Unity Editor, no serializando YAML a mano.
- Unity 6000.5.5f1 compiló el script en modo batch, ejecutó `Generate`, guardó la escena y confirmó los objetos requeridos.
- Se inspeccionó la serialización de la escena: cámara ortográfica, objetos requeridos, colliders de bloques de tamaño local 1×1, CapsuleCollider2D del jugador y Rigidbody2D. El sprite placeholder es un asset local, no externo.
- El diagnóstico de VS Code no detecta errores en el script.
- No se ejecutó Play Mode ni se tomó una captura de pantalla; la composición visual y el contacto real entre colliders requieren comprobación manual en el Editor.

### Pasos pendientes

- Abrir `Assets/_Project/Scenes/Test/MovementTest.unity` y comprobar cámara, encuadre, colores y colisiones en Play Mode.
- Revisar y aprobar el mapa propuesto en `Docs/Region1Plan.md` antes de construir salas.
- La siguiente tarea puede abordar el controlador de movimiento básico, de forma aislada y con pruebas específicas.

## Tarea 02: Auditoría y movimiento básico

### Auditoría de carpetas

- `Assets/_Project/00_Scripts/Editor` contiene la herramienta de generación/configuración de `MovementTest`; se conserva en su ubicación porque es código de Editor existente.
- `Assets/_Project/05_Art/Placeholder` contiene el sprite cuadrado gratuito usado por la escena.
- `Assets/_Project/Scenes/Test` contiene la escena de prueba solicitada en la Tarea 01. `04_Scenes`, `03_Prefabs`, `06_Audio` y `07_Settings` permanecen vacías; no se borraron.
- `01_Data` estaba vacía y ahora contiene `Player/PlayerMovementData.asset`.
- Se crearon los scripts en la ruta explícita solicitada `Scripts/Player`. La estructura numerada anterior (`00_Scripts`, `01_Data`, etc.) se conserva; no se movieron ni renombraron assets para resolver diferencias de convención.
- No se encontraron prefabs, ScriptableObjects ni scripts de juego preexistentes que duplicaran esta implementación. No se modificaron paquetes, ajustes globales, recursos internos ni el GDD.

### Implementación

- `Assets/_Project/Scripts/Player/PlayerController.cs`: lectura de Move/Jump/Sprint, aceleración, frenado, inversión rápida, orientación visual, salto variable, coyote time, jump buffer, comprobación de suelo por LayerMask y límite de caída.
- `Assets/_Project/Scripts/Player/PlayerMovementData.cs`: ScriptableObject con parámetros de movimiento.
- `Assets/_Project/01_Data/Player/PlayerMovementData.asset`: configuración inicial conectada al personaje.
- `Assets/_Project/00_Scripts/Editor/MovementTestSceneGenerator.cs`: se añadió un comando de Editor para enlazar controlador y asset sin reconstruir la escena; también agrega un techo de prueba solo si no existe.
- `Assets/_Project/Scenes/Test/MovementTest.unity`: mantiene las superficies previas, integra el controlador y añade el techo de prueba. Usa `Default` como capa de suelo.
- Se reutilizó `Assets/Settings/InputSystem_Actions.inputactions`: Move incluye stick, cruceta, A/D y flechas; Jump incluye Espacio y botón inferior; Sprint usa Shift y pulsación del stick izquierdo.

### Valores iniciales

- Caminar `4.5`, correr `7.5`, aceleración `50`, frenado `60`, giro `90`.
- Salto `13`, coyote time `0.12 s`, jump buffer `0.12 s`, corte al soltar `0.45`.
- Gravedad del Rigidbody2D `3`, caída máxima `18`, distancia de detección de suelo `0.14`, ancho relativo `0.85`.

### Verificación

- Unity `6000.5.5f1` compiló los ensamblados de juego y Editor; Tundra terminó correctamente sin errores C# en el log.
- El comando de Editor creó el asset y verificó las referencias serializadas, collider y Rigidbody2D del jugador. La escena guarda el `LayerMask` `Default` y conserva las plataformas.
- No se probó en Play Mode. Aceleración, frenado, inversión, salto variable, coyote time, jump buffer y colisiones deben comprobarse manualmente en Unity.

### Siguiente paso

- Revisar la escena en Play Mode con los pasos indicados al entregar esta tarea. Si la detección de suelo o el arco del salto requieren ajuste, cambiar los valores del ScriptableObject antes de modificar el controlador.
- La Tarea 03 puede comenzar tras validar estos fundamentos; dash, doble salto, pogo y combate permanecen fuera de esta entrega.

## Tarea 02.1: El personaje se suspende contra plataformas

### Diagnóstico

- El CapsuleCollider2D del jugador y los colliders rectangulares de las plataformas no tenían Physics Material 2D asignado (`m_Material` nulo); `Physics2DSettings` tampoco define un material predeterminado propio. Al empujar horizontalmente contra una cara vertical, la fricción de contacto predeterminada podía oponerse a la gravedad y dejar al jugador suspendido.
- `PlayerController.FixedUpdate` no anula la velocidad vertical al tocar una pared: conserva `body.linearVelocity.y`, salvo al iniciar un salto o limitar la caída máxima.
- La comprobación de suelo es un OverlapBox bajo los pies, con un ancho del 85% de los bounds del capsule. En el contacto lateral vertical de esta escena queda dentro del ancho del personaje, por lo que la pared no entra en esa consulta. No se modificó esa detección.

### Corrección

- Se creó `Assets/_Project/01_Data/Player/PlayerNoFriction.physicsMaterial2D` desde la API de Unity Editor.
- El material establece fricción y rebote en `0`, y combina ambos con `Minimum` para evitar reintroducir fricción/rebote al combinar con colliders sin material.
- Se asignó únicamente al CapsuleCollider2D de `Salamander Placeholder`. No se cambiaron código ni valores de aceleración, velocidad o salto.
- Se modificó el comando de Editor de `MovementTestSceneGenerator` para crear/reutilizar el material, asignarlo y verificar el estado guardado.

### Verificación

- Unity `6000.5.5f1` recompiló los ensamblados sin errores y ejecutó el comando de aplicación; el log terminó con código de salida `0`.
- Se verificó el asset serializado: fricción `0`, rebote `0`, combinación mínima y referencia asignada al CapsuleCollider2D en `MovementTest`.
- No se ejecutó Play Mode después de aplicar el cambio. La ausencia de suspensión y el contacto real con suelo, laterales y techo necesitan la prueba manual.

### Prueba manual pendiente

- En `MovementTest`, entrar en Play Mode y saltar hacia el lateral izquierdo de una plataforma manteniendo D; repetir contra el lateral derecho manteniendo A. En ambos casos debe seguir cayendo sin soltar la dirección.
- Aterrizar en la parte superior y caminar sobre suelo/plataforma; confirmar que permanece apoyado y que el frenado del controlador evita deslizamiento anormal.
- Saltar debajo de una plataforma y confirmar que su cara inferior detiene el ascenso.
- Revisar la consola de Unity para errores de compilación o ejecución.

## Tarea 02.2: Estructura sin numeración

### Estructura final

```text
Assets/_Project/
	Animations/
	Art/
		Placeholder/MovementTestSquare.asset
	Audio/
	Data/
		Player/PlayerMovementData.asset
		Player/PlayerNoFriction.physicsMaterial2D
	Materials/
	Prefabs/
	Scenes/
		Test/MovementTest.unity
	Scripts/
		Editor/MovementTestSceneGenerator.cs
		Player/PlayerController.cs
		Player/PlayerMovementData.cs
	Settings/
	Tests/
```

`Animations`, `Audio`, `Materials`, `Prefabs`, `Settings` y `Tests` están vacías, pero se conservaron/crearon como categorías previstas. No se eliminaron por estar vacías.

### Movimientos realizados

- `00_Scripts/Editor` se fusionó en `Scripts/Editor`; `MovementTestSceneGenerator.cs` y sus `.meta` se movieron mediante `AssetDatabase.MoveAsset`.
- `01_Data` se trasladó a `Data`.
- `03_Prefabs` se trasladó a `Prefabs`.
- `04_Scenes` estaba vacía y se fusionó con `Scenes`; su raíz se retiró después de verificar que quedó vacía.
- `05_Art` se trasladó a `Art`.
- `06_Audio` se trasladó a `Audio`.
- `07_Settings` se trasladó a `Settings`.
- `Animations`, `Materials` y `Tests` se crearon con la API de carpetas de Unity.
- `00_Scripts` quedó vacío al mover `Editor`; se retiró entonces mediante AssetDatabase. No quedaron carpetas numeradas.
- No hubo conflictos de nombres ni assets que debieran quedarse sin mover. El migrador temporal se retiró tras completar la tarea.

### Referencias y compilación

- La migración capturó los GUIDs del árbol antes de mover y comprobó que siguieran resolviendo después, exceptuando solo las raíces vacías fusionadas y retiradas.
- Se compararon además los `.meta` del generador de Editor, controlador, ScriptableObject, asset de datos, material físico, sprite placeholder y escena; todos conservan sus GUID originales.
- Unity volvió a abrir `Scenes/Test/MovementTest.unity` y verificó `PlayerController`, sus referencias de datos e Input Actions, el material del jugador y los objetos de suelo/plataformas/pared/techo.
- Unity 6000.5.5f1 compiló antes y después del traslado, y también después de retirar el migrador temporal. Tundra terminó con código 0 y sin errores C#.
- El generador actualiza las rutas de datos y arte a `Data` y `Art`. No se modificó el código de movimiento ni el GDD.

## Tarea 03: Dash y doble salto

### Implementación

- `Assets/_Project/Scripts/Player/PlayerAbilities.cs` mantiene dash y doble salto desactivados por defecto y permite configurar su disponibilidad desde el Inspector.
- `PlayerController` conserva el movimiento horizontal y la detección de suelo existentes; el dash suspende temporalmente la velocidad/gravedad normal, usa la orientación actual y restaura la gravedad al finalizar. Se permite un dash por periodo aéreo, con recuperación al aterrizar y cooldown.
- El doble salto reutiliza el buffer y la detección de suelo/coyote existentes. Solo se habilita tras un salto normal o de coyote; consume el salto aéreo adicional una sola vez y se reinicia al aterrizar.
- La velocidad horizontal ahora es única y configurable (`7.5`), equivalente al máximo anterior; Shift deja de ser sprint y pasa a Dash.
- `InputSystem_Actions.inputactions`: Sprint se sustituyó por Dash, asignado a `Left Shift` y `Gamepad/rightShoulder`. Jump conserva Espacio y botón inferior.
- `MovementTest` tiene ambos flags de `PlayerAbilities` activos para prueba. Una instancia nueva de `PlayerAbilities` mantiene ambas opciones apagadas.
- El hueco existente sirve para ensayar el dash; no se reconstruyó la escena ni se cambió su material sin fricción. La invulnerabilidad durante dash queda como parámetro configurable, sin sistema de daño.

### Parámetros iniciales

- Velocidad horizontal `7.5`; aceleración `50`, frenado `60`, giro `90`.
- Dash: distancia `3.2`, velocidad `18`, duración máxima `0.18 s`, cooldown `0.35 s`; invulnerabilidad configurable, activa como opción de datos, pero aún sin sistema que la aplique a daño.
- Doble salto: velocidad vertical `12`; salto normal conserva `13` y los valores existentes de corte, coyote y buffer.

### Verificación

- Unity 6000.5.5f1 compiló ensamblados de juego y Editor sin errores; Tundra terminó con código `0`.
- El configurador volvió a abrir/guardar la escena y verificó la configuración de datos/acciones; el JSON de Input Actions se parseó y se comprobó que Dash tiene únicamente Shift y el hombro derecho.
- La escena serializa `PlayerAbilities` con ambos flags activos y conserva la referencia al controlador, los datos de movimiento, el material sin fricción y las plataformas.
- No se ejecutó Play Mode. Dash en suelo/aire, límite y recuperación aérea, doble salto/límite del tercer salto, combinación de habilidades, colisión con paredes y caída sin adherencia quedan pendientes de la prueba manual.

### Pruebas manuales pendientes

- Con ambas opciones activas en `MovementTest`, probar dash en suelo y aire; repetir dash en el mismo periodo aéreo (debe bloquearse), aterrizar y probarlo de nuevo.
- Saltar y pulsar Espacio/botón inferior una segunda vez; intentar un tercer salto antes de aterrizar y confirmar que no ocurre. Repetir combinando dash y doble salto.
- Probar dash contra pared y junto a laterales de plataformas; comprobar que no atraviesa paredes ni suspende al personaje, y que la gravedad vuelve al terminar.
- Desactivar ambos flags desde `PlayerAbilities` y comprobar que el movimiento y salto normal siguen operativos sin dash ni salto extra.
- Revisar la consola de Unity durante estas pruebas.

## Tarea 04: Cámara de seguimiento 2D

### Decisión e implementación

- Cinemachine no está declarado en `Packages/manifest.json` ni instalado en `Library/PackageCache`; no se instaló ningún paquete.
- Se creó `Assets/_Project/Scripts/Camera/CameraFollow2D.cs`, implementado con `LateUpdate` y `Vector3.SmoothDamp`. Tiene objetivo, collider de límites, zona muerta X/Y y suavidad editables desde el Inspector.
- Los límites usan el bounds de un `BoxCollider2D` trigger; la cámara se clampa considerando el tamaño ortográfico y aspect ratio para no mostrar fuera de la sala. El trigger no participa en las colisiones del jugador.
- `MovementTest` conserva la cámara ortográfica, Renderer 2D, plataformas y material sin fricción. Se añadió `Camera Room Bounds` con tamaño `28 x 20`, centrado en `(0, 1.2)`, y se enlazó al seguimiento.
- Se configuró una zona muerta de `1.2 x 0.8`, suavidad `0.12 s`, y `Rigidbody2D.interpolation = Interpolate` solo para reducir jitter visual. No se modificaron scripts ni datos de movimiento.

### Verificación

- Unity 6000.5.5f1 compiló el componente y el código de Editor; Tundra terminó con código `0` y sin errores C#.
- El configurador abrió y guardó la escena existente; se verificaron la cámara ortográfica/tamaño, el objetivo, los límites trigger y la interpolación del Rigidbody2D.
- No se ejecutó Play Mode. El seguimiento al caminar, dash, saltos, caídas, límites y jitter deben probarse manualmente.

### Pruebas manuales pendientes

- Abrir `Scenes/Test/MovementTest.unity` y entrar en Play Mode; caminar a ambos lados y comprobar que la cámara espera dentro de la zona muerta y sigue con suavidad al salir de ella.
- Probar dash, salto normal, doble salto y caída desde plataformas; comprobar que el jugador permanece visible y que no hay vibración.
- Recorrer los cuatro bordes de `Camera Room Bounds` y verificar que el viewport no muestra fuera del rectángulo.
- En Inspector, variar `Dead Zone Size`, `Smooth Time` y el BoxCollider2D para comprobar que la configuración por sala es utilizable.
- Revisar la consola de Unity durante las pruebas.

## Tarea 05: Sistema de pogo

### Estado previo

- Se leyeron `AGENTS.md`, `Docs/GDD.md` y este registro. La copia local estaba limpia en `main` y contiene el controlador, dash, doble salto, cámara y `MovementTest` de las tareas anteriores. No había código de pogo ni objetivos compatibles.
- Se conservó `PlayerMovementData.asset` y no se modificaron los valores de movimiento existentes. El GDD no se modificó.

### Implementación

- `PlayerPogo` usa las acciones `Move` y `Attack` del Input System. Requiere estar en el aire, mantener abajo y pulsar ataque. Ejecuta un descenso y detecta objetivos solo durante la ventana configurada. Permite un intento por periodo aéreo; un impacto válido habilita otro y aterrizar también reinicia la disponibilidad.
- `PlayerPogoData` define ventana de ataque, velocidad de descenso, velocidad de rebote, desplazamiento y tamaño de la zona de impacto. El estado temporal permanece en `PlayerPogo`.
- `PogoTarget` marca colliders compatibles sin introducir vida, daño ni lógica de enemigos. El mismo objetivo no puede recibir impactos repetidos durante un ataque.
- `PlayerController` recibió solo dos métodos de física para iniciar el descenso y aplicar el rebote. El rebote conserva el movimiento horizontal y no restaura dash ni doble salto. También limpia el salto almacenado y el corte de salto pendiente para que no anulen el impulso vertical.
- Se añadió `J` a la acción `Attack`; se conserva el botón oeste del mando. La cruceta del mando se añadió a `Move` para permitir mantener abajo.
- `MovementTest` incluye `PlayerPogo`, su asset de datos y dos objetivos dorados estáticos con colliders trigger, para que el controlador no los confunda con suelo. `PogoTestSceneSetup` permite volver a aplicar esta configuración desde `Fangbound/Configure Movement Test Pogo` sin reconstruir la escena.

### Verificación realizada

- Unity `6000.5.5f1` compiló los scripts y ejecutó `PogoTestSceneSetup.Configure` en modo batch; el proceso terminó con código `0` y confirmó los dos objetivos en `MovementTest`.
- Se comprobó la serialización de `PlayerPogo`, sus referencias al asset de datos y las acciones, y los componentes `PogoTarget` de ambos objetivos.
- La primera compilación detectó un uso incorrecto de `ContactFilter2D.NoFilter`; se corrigió y la compilación final no mostró errores C#.
- No se ejecutó Play Mode. El tacto del descenso, el rebote y las combinaciones de habilidades aún requieren prueba manual.

### Prueba manual pendiente

1. Abrir `Assets/_Project/Scenes/Test/MovementTest.unity` y entrar en Play Mode. Saltar sobre el primer objetivo, mantener `S` o flecha abajo y pulsar `J`; comprobar descenso y rebote vertical.
2. Dirigir el rebote con `A/D` o flechas y hacer otro pogo sobre el segundo objetivo sin tocar el suelo. En mando: stick o cruceta abajo más botón oeste; mover con stick o cruceta.
3. Atacar hacia abajo lejos de los objetivos; debe haber descenso sin rebote y no se debe poder repetir el pogo hasta acertar o aterrizar.
4. Mantener abajo y pulsar ataque desde el suelo; no debe iniciar el descenso. Probar también un ataque en el aire sin mantener abajo.
5. Combinar pogo con doble salto y dash antes y después de un impacto. Confirmar que el pogo no devuelve usos consumidos y que el control horizontal vuelve tras el rebote.
6. Revisar la consola para errores de ejecución y ajustar únicamente `PlayerPogoData.asset` si la ventana o la zona de impacto requieren calibración.

### Limitaciones actuales

- Solo los colliders con `PogoTarget` producen rebote. El GDD contempla suelo, pinchos y enemigos; su integración queda para las tareas correspondientes.
- No hay animación ni efecto visual de ataque. La zona se puede ver como gizmo al seleccionar al jugador en Scene View.

## Tarea 06: Combate básico y ajuste del pogo

### Ajuste del pogo

- `PlayerController.TryStartPogoDive` ya no recibe velocidad de caída ni modifica `linearVelocity.y`; al iniciar el ataque solo limpia coyote time, jump buffer y corte pendiente. La caída queda a cargo de la gravedad normal.
- Se conserva la ventana/hitbox de `PlayerPogo`, la condición en aire, el rebote de `BounceFromPogo`, el límite/reinicio del pogo y los controles abajo + J/botón oeste.
- Se retiraron `DiveSpeed` y `diveSpeed` del componente/datos. Unity reserializó `PlayerPogoData.asset` sin el campo obsoleto; `attackWindow`, rebote y hitbox se conservaron.

### Combate y objetivos

- `Assets/_Project/Scripts/Player/PlayerCombat.cs` lee Move/Attack del Input System. J sin dirección vertical ataca horizontalmente hacia la orientación actual; izquierda/derecha + J fijan la dirección; arriba + J usa el hitbox ascendente. Abajo + J se reserva para pogo en aire y no produce ataque/impulso descendente en suelo.
- PlayerCombat mantiene los objetivos ya golpeados durante cada ventana, evitando impactos repetidos sobre el mismo objetivo. Las zonas horizontal/ascendente, daño y ventana están en `PlayerCombatData`.
- `Assets/_Project/Scripts/Combat/IDamageable.cs` separa la recepción de daño del ataque. `TrainingTarget` implementa vida configurable por componente, flash de color al recibir daño, degradado visual al perder vida y desactivación de collider al agotarse.
- `Assets/_Project/Scripts/Editor/CombatTestSceneSetup.cs` configura el jugador/asset y añade dos blancos reutilizables para probar los ataques. Los dos objetivos dorados `PogoTarget` existentes no se reemplazaron.
- `PlayerCombat` es dueño de habilitar/deshabilitar `Attack`; `PlayerController` sigue gestionando `Move`, que usan tanto combate como pogo.

### Configuración inicial

- PlayerCombatData: daño `1`, ventana `0.12 s`, hitbox horizontal offset `(0.65, 0.05)`, tamaño `(0.85, 0.55)`; ascendente offset `(0, 0.75)`, tamaño `(0.6, 0.8)`.
- Cada objetivo de entrenamiento empieza con `3` puntos de vida y flash visual `0.1 s`.
- Se conservan valores de movimiento, dash, salto, cámara y material sin fricción.

### Verificación

- Unity `6000.5.5f1` compiló el pogo corregido, el combate, los objetivos y el configurador; Tundra terminó con código `0` y sin errores C#.
- El configurador abrió y guardó `MovementTest`, verificó referencias de `PlayerCombat` y `PlayerPogo`, creó el asset de datos y comprobó los blancos de entrenamiento y los dos `PogoTarget` existentes.
- Se comprobó que la escena conserva el material sin fricción, y que `PlayerPogoData.asset` ya no serializa `diveSpeed`.
- No se ejecutó Play Mode; los casos de combate y la sensación de caída/rebote requieren prueba manual.

### Pruebas manuales pendientes

- En Play Mode, mantener abajo + J en el aire: confirmar caída natural sin aceleración añadida y rebote solo al golpear un objetivo dorado. Repetir lejos de objetivos y desde el suelo; no debe haber rebote ni impulso descendente artificial.
- Usar J quieto y derecha/izquierda + J para golpear horizontalmente; comprobar el lado al que mira y que el objetivo pierde vida una sola vez por ataque.
- Usar arriba + J en suelo y aire; confirmar el impacto en el objetivo superior sin impulso automático del jugador.
- Confirmar que abajo + J en aire prioriza pogo, abajo + J en suelo no ataca ni impulsa, y dash/doble salto conservan su comportamiento.
- Repetir ataques hasta agotar los 3 puntos de vida; observar flash/degradado y que el objetivo deja de recibir impactos.
- Revisar la consola de Unity durante las pruebas.

## Tarea 07: Parry y postura

### Implementación

- Se añadió la acción `Parry` al mapa `Player`: K y hombro izquierdo del mando. No comparte botón con Attack/J, Jump/A, ni Dash/Shift/hombro derecho; se activa por `WasPressedThisFrame`, no al mantenerlo.
- `PlayerParry` gestiona una ventana finita, recuperación completa posterior al éxito/fallo y feedback provisional de éxito/fallo/impacto. Puede usarse en suelo y aire.
- El impacto se detecta por la hitbox activa del ataque enemigo. `EnemyAttackInstance` se resuelve una sola vez; `PlayerImpactReceiver` devuelve `Parried`, `Hit`, `AvoidedByDash` o `Ignored`. Dash evita el impacto sin contarlo como parry y sin dar postura.
- `EnemyPosture` separa postura de vida. Cada parry exitoso reduce postura; a cero activa vulnerabilidad temporal. Al terminar, postura se recupera progresivamente; un nuevo parry durante recuperación parcial la vuelve a reducir. La vulnerabilidad activa no acumula postura.
- `TrainingAttackEmitter` ejecuta ciclos estáticos de espera → telegraph → hitbox activa → cooldown. No persigue ni navega; el telegraph visual representa la misma zona de impacto.
- Se añadieron `EnemyPostureData`, `TrainingAttackData` y `PlayerParryData` para los parámetros. El receptor del jugador solo comunica el impacto; no introduce vida, muerte ni respawn del jugador.
- `MovementTest` conserva combate y pogo previos, ambos objetivos de pogo, los blancos horizontal/ascendente y el material sin fricción. Se añadió el entrenador de parry con postura y telegraph.

### Parámetros iniciales

- Parry: ventana `0.15 s`, recuperación `0.4 s`, feedback visual `0.12 s`.
- Postura: máximo `3`, daño por parry `1`, vulnerabilidad `1.5 s`, recuperación `1 punto/s`.
- Entrenador: espera inicial `0.75 s`, telegraph `0.65 s`, ataque activo `0.14 s`, cooldown `1.3 s`. La zona se configura con offset `(-1.8, -0.05)` y tamaño `(3.6, 0.75)`.

### Verificación

- Unity 6000.5.5f1 compiló los componentes de parry/postura, receptor/emisor y el configurador; Tundra terminó con código `0` y sin errores C#.
- `CombatTestSceneSetup.Configure` abrió/guardó `MovementTest` y verificó referencias de PlayerCombat, PlayerParry, PlayerPogo, datos de parry/postura/ataque, receiver, emisor y telegraph.
- La escena contiene dos `PogoTarget`, dos blancos `IDamageable`, el entrenador `EnemyPosture`/`TrainingAttackEmitter` y el material sin fricción. No se cambió movimiento, dash, salto ni cámara.
- No se ejecutó Play Mode; timing de parry, postura, vulnerabilidad y la interacción efectiva con dash quedan pendientes de prueba manual.

### Pruebas manuales pendientes

- Entrar en Play Mode y pulsar K o hombro izquierdo antes del golpe: el flash de éxito debe aparecer, no debe haber impacto y la postura del entrenador debe bajar una vez.
- Pulsar demasiado pronto: el flash de fallo debe aparecer al cerrar la ventana y el siguiente ataque debe impactar. Pulsar después del impacto tampoco debe marcar parry.
- Mantener K: debe producir un solo intento; probarlo en suelo y aire.
- Acertar tres parries: comprobar postura `3 → 0`, color vulnerable durante `1.5 s` y recuperación gradual hasta `3`.
- Recibir ataque durante dash: no debe recibir impacto ni reducir postura. Probar también el feedback de impacto fuera de dash/parry.
- Confirmar que Attack/J, pogo, dash, doble salto y movimiento siguen respondiendo; revisar la consola.

## Tarea 08: Vida, daño, invulnerabilidad y knockback

### Implementación

- `PlayerHealthData` separa parámetros de diseño y `PlayerHealth` mantiene vida/estados temporales. Vida máxima e inicial: `5`; API pública `TryHeal`; eventos `HealthChanged`, `DamageTaken`, `Healed` y `Died`.
- `PlayerImpactReceiver` sigue siendo el único receptor. Orden de resolución: ataque ya resuelto → muerte → dash invulnerable → i-frames posteriores al golpe → parry → daño real. Un impacto en dash no consume parry ni daña; uno durante i-frames tampoco puede registrarse como parry.
- Los impactos válidos aplican daño, `0.8 s` de invulnerabilidad visual y knockback con control bloqueado temporalmente. El emisor suministra posición/damage mediante `EnemyAttackInstance`; no se introduce un segundo sistema de recepción.
- Al llegar a cero, el jugador queda bloqueado, se detienen sus componentes de combate/pogo/parry, el cuerpo queda inmóvil y la HUD muestra estado de derrota. `R` reinicia vida, posición de `Player Spawn Point`, velocidad, gravedad y estados temporales; dash/doble salto siguen disponibles según las opciones actuales de `PlayerAbilities`.
- `PlayerHealthUI` usa IMGUI provisional para mostrar bloques de vida y el aviso de muerte, sin recursos gráficos adicionales.
- `MovementTest` se configuró con `PlayerHealth`, `PlayerHealthUI` y el asset de vida. No se modificaron movimiento, dash, salto, cámara, GDD ni estructura de carpetas.

### Parámetros iniciales

- Vida máxima: `5`.
- Invulnerabilidad tras daño: `0.8 s`; parpadeo visual: `0.08 s`.
- Knockback horizontal: `7`; vertical: `4.5`; bloqueo de control: `0.28 s`.
- Daño del ataque de entrenamiento: `1` por ataque activo.

### Verificación

- Unity `6000.5.5f1` compiló el núcleo de salud y ejecutó `CombatTestSceneSetup.Configure`; Tundra terminó con código `0`.
- Se verificaron las referencias serializadas de salud, Input Actions, spawn y HUD; la escena conserva `PlayerImpactReceiver`, parry/postura, enemigos de entrenamiento, objetivos de pogo y el material sin fricción.
- El Input System contiene una única acción `Restart` ligada a R. El receptor comprueba dash e invulnerabilidad antes de consumir la ventana de parry.
- No se ejecutó Play Mode; daño, i-frames, knockback, muerte y reinicio requieren prueba manual.

### Pruebas manuales pendientes

- Recibir un ataque sin parry: vida baja de `5` a `4`, aparece knockback/parpadeo y los golpes de los siguientes `0.8 s` se ignoran.
- Parryar otro ataque y comprobar vida sin cambio ni knockback; fallar el parry y comprobar daño normal.
- Activar dash durante la hitbox: no debe recibir daño ni registrar parry ni postura.
- Recibir daño hasta cero: comprobar bloqueo, aviso de derrota y HUD; pulsar R y verificar posición, vida, gravedad, velocidades, controles e invulnerabilidad restaurados.
- Tras reiniciar, probar ataque horizontal/ascendente, pogo y doble salto para comprobar que los componentes se reactivan.
- Revisar la consola de Unity durante las pruebas.

## Tarea 09: Enemigo terrestre (continuación del trabajo interrumpido)

### Auditoría inicial

- Se leyeron `AGENTS.md`, `GDD.md`, este registro y `Region1Plan.md`. El último commit era `891b90d` (Tarea 08); se respetó su condición de completada y probada indicada por el usuario.
- Git solo mostraba dos archivos modificados (`EnemyPosture.cs`, `TrainingAttackEmitter.cs`) y el nuevo `EnemyAttackHitbox.cs` con su `.meta`. No había controlador terrestre, datos propios, vida de enemigo, prefab ni integración de Tarea 09 en la escena.
- La extracción de la hitbox era una base válida, pero la migración estaba incompleta: el emisor requería una referencia todavía ausente en MovementTest; el configurador seguía buscando el campo retirado `posture`. Compilar esos scripts no garantizaba que el entrenador pudiera ejecutarse.
- La hitbox calculaba `SourcePosition` a partir de los bounds de un collider desactivado, que no ofrecen una geometría válida. Además, excluir todo `transform.root` podía excluir al jugador si ambos estaban bajo un grupo común. Resolver el golpe también adelantaba el cooldown del entrenador, alterando sus ciclos.
- El indicador visual opcional de `EnemyPosture` era válido y se conservó, con el renderer original como alternativa para el entrenador.

### Archivos y comportamiento

- `Assets/_Project/Scripts/Combat/EnemyAttackHitbox.cs`: se completó la extracción compartida. Usa la forma de un `BoxCollider2D`, calcula centro/tamaño mediante transforms y consulta física durante el ataque. El collider permanece desactivado: no añade una zona donde el enemigo pueda recibir golpes a distancia ni produce contactos adicionales. Cada ataque usa un único `EnemyAttackInstance`; comunica el resultado y aplica postura solo en `Parried`. Filtra el propietario, no todo el grupo de escena, y cancela el ataque al desactivarse.
- `Assets/_Project/Scripts/Combat/TrainingAttackEmitter.cs`: usa la hitbox común y conserva las fases/tiempos originales. Un impacto resuelto no acorta su fase activa. Al desactivarse limpia hitbox y aviso.
- `Assets/_Project/Scripts/Combat/EnemyPosture.cs`: se conservó el cambio previo del indicador opcional, sin reescribir las reglas de postura o recuperación.
- `Assets/_Project/Scripts/Enemies/GroundEnemyData.cs` y `Data/Enemies/GroundEnemyData.asset`: parámetros de vida, velocidades, detección, ataque, multiplicador vulnerable y feedback. Los estados temporales quedan en componentes, no en el asset.
- `Assets/_Project/Scripts/Enemies/EnemyHealth.cs`: implementa `IDamageable`, flash de daño y vida independiente de la postura. Durante vulnerabilidad recibe daño multiplicado. A cero cancela ataques, desactiva colliders/postura/simulación física y emite `Died` una sola vez. El cuerpo queda visible con color de muerte.
- `Assets/_Project/Scripts/Enemies/GroundEnemyController.cs`: estados `Patrol`, `Chase`, `Telegraph`, `Attack`, `Recovery`, `Vulnerable` y `Dead`. Detecta por proximidad horizontal y tolerancia vertical; pierde al jugador por distancia, altura, desactivación o muerte. Fija orientación al comenzar el aviso. La ruptura de postura cancela el ataque y detiene la IA; al terminar la vulnerabilidad pasa por recuperación y retoma patrulla/persecución. Los límites horizontales restringen también la persecución; detecta suelo y obstáculos para no caminar fuera de una plataforma.
- `Assets/_Project/Prefabs/Enemies/GroundEnemy.prefab`: prefab reutilizable con cuerpo, vida, postura, controlador, indicador de orientación, indicador de postura y zona de aviso/ataque. Usa el sprite provisional y el material sin fricción existentes. En otras escenas puede enlazarse `target`; si está vacío busca un `PlayerHealth` al iniciar.
- `Assets/_Project/Scripts/Editor/CombatTestSceneSetup.cs`: se actualizó únicamente la creación/verificación del entrenador para enlazar la hitbox compartida. El aviso ahora coincide con la zona física original en unidades de mundo, compensando la escala del entrenador.
- `Assets/_Project/Scripts/Editor/GroundEnemySceneSetup.cs`: menú `Fangbound/Configure Movement Test Ground Enemy`. Migra el entrenador, crea los assets nuevos si faltan e incorpora una instancia del prefab sin ejecutar el configurador general del jugador. Comprueba contratos, prefab, objetivos anteriores y ausencia de scripts faltantes.
- `Assets/_Project/Scripts/Editor/GroundEnemyValidation.cs`: menú `Fangbound/Validate Ground Enemy in Play Mode`; regresión automatizada con la escena real. Se ejecuta desde Edit Mode, abre MovementTest y termina saliendo de Play Mode; en batch cierra el Editor con código de resultado. Las modificaciones de prueba solo ocurren en Play Mode y no se guardan en la escena.
- `Assets/_Project/Scenes/Test/MovementTest.unity`: entrenador migrado y enemigo morado en el suelo derecho, inicialmente `(4.5, -1.58)`. Patrulla aproximadamente entre `x=1.4` y `x=6.8`. Conserva jugador, spawn, cámara, geometría, HUD, dos blancos de combate y dos objetivos de pogo.
- Se añadieron los `.meta` de los scripts, carpeta `Enemies`, datos y prefab nuevos. Este registro documenta la entrega. No se modificaron scripts/datos/Input Actions del jugador, paquetes, GDD ni el plan de región. No se realizaron commits ni push.

### Parámetros iniciales

| Parámetro | Valor |
| --- | --- |
| Vida del enemigo | 4 |
| Daño recibido durante vulnerabilidad | x2 |
| Velocidad de patrulla / persecución | 1.3 / 2.6 unidades/s |
| Detección / pérdida horizontal | 3.5 / 5.5 unidades |
| Tolerancia vertical | 1.2 unidades |
| Alcance para iniciar ataque | 1.3 unidades |
| Daño / aviso / activo / recuperación | 1 / 0.65 s / 0.14 s / 1.3 s |
| Offset / tamaño de ataque | (0.85, 0) / (1.2, 0.8), reflejados según orientación |
| Postura compartida | 3 puntos; 1 por parry; vulnerable 1.5 s; recupera 1 punto/s |

### Migración del entrenador

- Se conserva la zona física anterior: centro `(-5.6, -1.4)` en MovementTest, tamaño `(3.6, 0.75)`, offset `(-1.8, -0.05)` respecto al entrenador. El aviso anterior se veía más pequeño por la escala `0.7` del padre; se corrigió su representación para mostrar la zona que ya dañaba.
- Se conservan espera inicial `0.75 s`, aviso `0.65 s`, fase activa completa `0.14 s`, cooldown `1.3 s`, daño `1` y la resolución única por ataque.
- La recepción sigue pasando por `PlayerImpactReceiver`: muerte, dash e i-frames mantienen prioridad sobre parry/daño. No hay segundo receptor ni cambios al sistema del jugador.

### Validación realizada

- Unity `6000.5.5f1` compiló los ensamblados de juego y Editor. El configurador generó el prefab y guardó MovementTest mediante la API de Unity, con `TASK09_SETUP_PASS` y salida `0` (`Logs/Task09Setup.log`).
- La prueba automatizada en Play Mode terminó con `TASK09_PLAYMODE_PASS` (`Logs/Task09PlayMode.log`): patrulla en ambos sentidos/límites, detección y persecución, lado fijo del aviso, daño único, retorno al perder al jugador, regresión del entrenador, tres parries del enemigo, vulnerabilidad x2, recuperación de postura, muerte durante aviso, evento único de muerte, colliders cancelados y cuerpo detenido. También se probó una plataforma aislada con bordes y una pared.
- La ejecución final está en `Logs/Task09Validation.log`, con `TASK09_PLAYMODE_PASS`, sin errores/avisos C# ni excepciones. Amplía la regresión a tres parries del propio entrenador, color vulnerable del renderer original y prioridad de i-frames frente a una ventana de parry abierta. Una pasada intermedia no observó ambos sentidos al medir el transform interpolado; se cambió la medición a `Rigidbody2D.position` y la ejecución completa posterior pasó. No se alteró la lógica de patrulla para obtener ese resultado.
- `git diff --check` pasó. La comparación con HEAD de los objetos/componentes serializados del jugador confirmó que no cambiaron; tampoco hay diferencias en sus scripts, datos, Input Actions ni ajustes de proyecto.
- La regresión usa física y componentes reales. Para aislar los casos inmoviliza/reposiciona al jugador, abre ventanas de parry mediante reflexión y llama a la consulta existente de `PlayerCombat`. La patrulla se mide con la posición física de `Rigidbody2D`, evitando depender del transform visual interpolado en modo sin gráficos. No reproduce pulsaciones humanas ni certifica el ajuste visual o la sensación de combate.
- El primer lanzamiento dentro del sandbox falló por conexión IPC con Package Manager; se repitió fuera del sandbox. No fue un error C# del proyecto. Se actualizaron las búsquedas de objetos de los archivos nuevos a las APIs vigentes para evitar los avisos de obsolescencia de Unity 6.5.

### Cómo probar manualmente en Unity

1. Abrir `Assets/_Project/Scenes/Test/MovementTest.unity` con Unity `6000.5.5f1`, limpiar la consola y entrar en Play Mode. La escena ya está configurada; no hace falta regenerarla. Seleccionar `Environment/Ground Enemy` y activar Gizmos en Scene View para ver límites y zona de detección. Los valores de vida/postura/estado son propiedades públicas comprobadas por la validación automática; no hay HUD de enemigo en esta entrega.
2. Sin acercarse, observar al enemigo morado en el suelo derecho: debe recorrer ambos sentidos, girar en sus límites y permanecer sobre el suelo. Acercarse usando el movimiento/dash/doble salto existentes: debe perseguir; alejarse más de `5.5` unidades o salir de su tolerancia vertical: debe volver a patrullar.
3. Entrar en su alcance. Observar el aviso rojizo de `0.65 s` y el ataque amarillo de `0.14 s`. Cambiar de lado durante el aviso: la zona no debe girar para seguir al jugador. Sin parry/dash, el impacto debe quitar una vida al jugador, con el knockback y feedback existentes, una sola vez por ataque.
4. Pulsar K o el hombro izquierdo justo antes del impacto. Cada parry correcto debe conservar la vida del jugador y reducir postura una vez (`3 → 2 → 1 → 0`). Al romperla, la franja superior debe mostrarse turquesa y el enemigo debe dejar de moverse/atacar durante `1.5 s`.
5. Golpear con J antes de romper postura: debe restar `1` de vida. Golpear durante la vulnerabilidad: debe restar `2`. Si se deja vivir, observar el fin de la vulnerabilidad, recuperación progresiva de postura y reanudación del comportamiento. La vida no se regenera por recuperar postura.
6. Agotar su vida con ataques: debe quedar gris/transparente, sin colisiones, movimiento, avisos ni nuevos impactos. Probar un golpe letal durante el aviso para comprobar su cancelación. Salir y volver a entrar en Play Mode para restaurar el enemigo; R conserva el reinicio existente del jugador y no resucita enemigos.
7. Repetir con el entrenador a la izquierda: comprobar el aviso ampliado hasta la zona física real, ciclos anteriores, daño `1`, dash sin daño ni postura y tres parries hasta vulnerabilidad con su color original. Probar también ataque horizontal/ascendente, pogo, salto, doble salto, dash, muerte/R y HUD como regresión manual; revisar la consola.
8. Para repetir la validación automática, salir de Play Mode y usar `Fangbound/Validate Ground Enemy in Play Mode`. La consola debe terminar con `TASK09_PLAYMODE_PASS`. Si se necesitan reconstruir los assets de Tarea 09, usar su menú de configuración específico; evitar el generador completo de MovementTest porque reconstruye la escena base.

### Limitaciones y pendientes

- Enemigo de prueba con sprites/colores provisionales, sin animaciones finales, audio, navegación compleja, saltos ni rutas entre plataformas. La detección es por proximidad, sin oclusión visual por paredes. Los límites pertenecen a cada instancia y los parámetros generales al asset.
- Solo usa ataques parryables y la reducción de postura por parry del contrato existente. No incorpora ataques rojos no parryables, ejecución, daño de contacto ni daño de pogo: esos sistemas no forman parte de esta entrega.
- Pendientes la revisión humana de visibilidad del aviso, sensación de combate y regresión completa con teclado/mando. No se generó ni ejecutó una build standalone.
- **Se conserva pendiente la verificación manual específica de los `0.8 s` de i-frames de `PlayerHealth`.** Se verificó que un impacto inmediato durante invulnerabilidad se ignora; no se midió la frontera temporal de `0.8 s`. Para cerrarla, usar ataques independientes antes y después de ese límite y confirmar vida/parry/feedback. No se modificó `PlayerHealth` ni su asset.

## Tarea 09.1: Corrección de empuje físico y recuperación de patrulla

### Estado y causa identificada

- El usuario confirmó la prueba manual satisfactoria de la Tarea 09: patrulla, detección, persecución, ataques, parry, postura, vulnerabilidad, vida y muerte. Esta corrección conserva esos sistemas y no inicia la Tarea 10.
- Jugador y enemigo tenían `Rigidbody2D` dinámicos, masa `1`, rotación congelada y colliders sólidos en `Default`. La matriz de colisiones 2D permitía todos los contactos. Asignar `linearVelocity.x` desde la IA no impide que el solver de contactos aplique impulsos/desplace el enemigo durante el paso físico posterior; el material sin fricción tampoco evita el impulso normal del contacto.
- `TryMove` rechazaba cualquier paso que terminase fuera de los límites de patrulla, incluso si acercaba al enemigo al intervalo válido. Tras ser empujado suficientemente lejos, ambos sentidos quedaban bloqueados y Patrol invertía orientación sin poder avanzar.
- En Chase, un movimiento bloqueado simplemente dejaba velocidad horizontal a cero y volvía a intentarse. No había una retirada temporal ante paredes/bordes/límites. Telegraph, Attack y Recovery podían seguir funcionando, explicando que un enemigo inmóvil continuase atacando.

### Solución y archivos de esta corrección

- `Assets/_Project/Scripts/Enemies/GroundEnemyController.cs`: excluye con `Physics2D.IgnoreCollision` únicamente los pares entre su collider corporal y los colliders sólidos del jugador objetivo. Se aplica al iniciar y al reactivar el componente. Ambos cuerpos siguen siendo dinámicos y sólidos contra el terreno; no se cambian masas, gravedad ni restricciones físicas. Los cuerpos del jugador/enemigo ahora pueden atravesarse, conservando las consultas independientes de ataques y daño.
- `ProjectSettings/TagManager.asset`: añade `EnemyBody` en el índice libre `8`. No modifica las capas existentes ni la Layer Collision Matrix. Esta separación es necesaria porque las consultas de suelo de `PlayerController` usan `OverlapBox` con máscara `Default`: ignorar contactos por pareja no impediría por sí solo que el enemigo fuese detectado como suelo al atravesarlo.
- `Assets/_Project/Prefabs/Enemies/GroundEnemy.prefab`: solo cambia la capa del objeto raíz que contiene el collider sólido a `EnemyBody`. Sus hijos visuales/hitbox y todas las referencias/parámetros se conservan. MovementTest hereda el cambio del prefab; no necesita regenerarse.
- `Assets/_Project/Scripts/Editor/GroundEnemySceneSetup.cs`: los prefabs nuevos usan `EnemyBody`; la verificación exige cuerpo sólido en esa capa y comprueba que no esté incluida en el suelo del jugador.
- La patrulla prioriza regresar al intervalo si el cuerpo está fuera, orientándose hacia él. `TryMove` permite pasos que se acerquen desde fuera y mantiene las comprobaciones de paredes y suelo. No teletransporta ni atraviesa paredes/bordes para volver.
- Una persecución bloqueada vuelve a Patrol, orientada para retirarse. Durante el tiempo de recuperación ya configurado (`1.3 s` inicialmente) pospone la nueva detección, evitando la alternancia inmediata Patrol/Chase frente al mismo obstáculo. Después puede volver a intentar perseguir. No se añaden estados de navegación ni pathfinding.
- Se conservan orientación y tiempos de los ataques comprometidos. Si el desplazamiento ocurre durante Telegraph/Attack/Recovery, completa el ciclo y después recupera su posición mediante Patrol. La vulnerabilidad mantiene su parada temporal y el daño recibido x2; no hay instakill ni ejecuciones del enemigo básico. Las ejecuciones que eliminan una barra completa se reservan para futuros jefes según la decisión del usuario.
- `Assets/_Project/Scripts/Editor/GroundEnemyValidation.cs`: amplía la regresión existente con acciones reales del Input System de caminar y dash en ambos sentidos, y desplazamientos forzados independientes de la prevención del empuje. La prueba dirige temporalmente la entrada al juego aun sin foco de ventana y restaura esas opciones en `finally`; no guarda cambios en Input Settings.
- `Docs/DevelopmentLog.md`: documenta esta corrección. No se modifican scripts/datos del jugador, Input Actions, salud/postura/daño de enemigo, parámetros de dash ni sistemas del entrenador. Se preservan todos los cambios locales de Tarea 09 anteriores a esta petición. No hay commits ni push.

### Validación automatizada

- Unity `6000.5.5f1` compiló y ejecutó la batería completa en el proyecto principal. `Logs/Task09CollisionValidation.log` terminó con `TASK09_COLLISION_PASS`, `TASK09_RECOVERY_PASS` y `TASK09_PLAYMODE_PASS`, sin errores C#, avisos C# ni excepciones en esa ejecución. `git diff --check` pasó.
- La regresión de colisiones comprueba contacto excluido solo entre cuerpos; terreno conservado; caminar/dash mediante A/D y Shift con el controlador real, en ambos sentidos; desplazamiento horizontal del enemigo menor de `0.03` unidades; cruce efectivo del jugador y velocidad de dash conservada; ausencia de daño de contacto; enemigo excluido del suelo consultado por el jugador; y exclusión de contacto restaurada al reactivar el enemigo.
- La regresión de recuperación fuerza al enemigo fuera de cada límite y comprueba que regrese, coloca el límite detrás de una pared para verificar una persecución realmente bloqueada por terreno y observa la retirada en Patrol. También fuerza el desplazamiento durante Telegraph y comprueba Attack → Recovery → Patrol y regreso al intervalo.
- Se conserva la batería anterior: patrulla normal, detección/persecución/ataque, pérdida del jugador, daño único, tres parries, aturdimiento y vulnerabilidad x2, recuperación de postura, muerte, bordes, paredes y regresión del entrenador con sus colores y resultados de impacto.
- El primer intento con teclado sintético no produjo el cruce esperado en modo sin ventana. Se ajustó únicamente el foco de entrada de la prueba, de forma temporal y reversible. No se alteró el movimiento para conseguir el resultado.
- Se preparó una copia ignorada por Git bajo `.utmp/Task09CollisionValidation` mientras el Editor principal estaba abierto. Su primer arranque registró una excepción interna del índice de búsqueda de Unity. Una vez cerrado el Editor principal, la validación de la corrección se ejecutó directamente en el proyecto principal; no se cerró ni terminó desde herramientas la sesión interactiva del usuario.

### Pruebas manuales recomendadas

1. Abrir MovementTest, comprobar que el objeto raíz del prefab GroundEnemy usa `EnemyBody` y entrar en Play Mode. Caminar y hacer dash a través del enemigo desde izquierda y derecha. Debe poder atravesarse sin empujarlo; el enemigo sigue sujeto a su IA y ambos cuerpos conservan el suelo.
2. Repetir cuando el enemigo esté avisando un ataque, recuperándose o vulnerable: el contacto corporal no debe desplazarlo. Saltar cerca y encima de él; su cuerpo atravesable no debe habilitar un falso salto desde el aire ni actuar como plataforma.
3. Alejarse para que patrulle. Durante Play Mode, mover temporalmente el cuerpo a `x=7.35`, `y≈-1.6` (entre límite derecho y pared), y luego a `x=0.95`, `y≈-1.6` (fuera del izquierdo pero todavía sobre suelo). Con el jugador lejos debe regresar a sus límites y continuar, sin teletransportarse. No guardar esos movimientos en Edit Mode.
4. Probar una pared o borde delante de una persecución: debe detener el avance peligroso y retirarse brevemente patrullando, con posibilidad de reintentar si el jugador sigue cerca. No puede alcanzar una zona separada por paredes/saltos que requieran navegación; no debe atravesar obstáculos para hacerlo.
5. Volver a acercarse para comprobar persecución, aviso y ataque. Acertar tres parries: postura `3 → 0`, aturdimiento de `1.5 s`, daño x2 durante vulnerabilidad y posterior recuperación. Agotar vida con ataques para comprobar muerte/cancelación; romper postura por sí solo no mata al enemigo.
6. Repetir el entrenador anterior y la regresión del jugador: daño, parry, dash invulnerable, salto/doble salto, ataque horizontal/ascendente, pogo, muerte/R y HUD. R continúa reiniciando solo al jugador. Revisar la consola.
7. Para repetir la batería automática, usar desde Edit Mode `Fangbound/Validate Ground Enemy in Play Mode`; debe terminar con `TASK09_COLLISION_PASS`, `TASK09_RECOVERY_PASS` y `TASK09_PLAYMODE_PASS`.

### Pendiente conservado

- La verificación manual de la frontera exacta de `0.8 s` de i-frames de `PlayerHealth` sigue abierta. La corrección no cambia su código ni sus datos. La regresión de impactos durante invulnerabilidad no sustituye esa medición temporal.

## Tarea 09.2: Integración del pogo con el enemigo terrestre

### Causa y alcance

- El usuario confirmó que la Tarea 09 y la corrección de colisiones/patrulla funcionan en sus pruebas manuales. El problema pendiente era que el ataque descendente atravesaba al enemigo sin rebotar.
- `PlayerPogo` ya detectaba su ventana descendente mediante `OverlapBox`, pero solo aceptaba colliders con `PogoTarget` en su jerarquía. El prefab terrestre tenía `EnemyHealth`/`IDamageable`, pero no esa marca. Además, el pogo de Tarea 05 solo producía rebote: todavía no enviaba daño a `IDamageable`.
- Las colisiones corporales sin empuje y la capa `EnemyBody` no causan el fallo: el pogo usa una consulta independiente con `ContactFilter2D.noFilter`. No es necesario restaurar contactos físicos para que el ataque acierte.

### Archivos de esta entrega

- `Assets/_Project/Scripts/Player/PlayerPogo.cs`: conserva el sistema existente de entrada, ventana, consulta, disponibilidad y rebote. Tras encontrar `PogoTarget`, busca opcionalmente `IDamageable`, descarta los receptores que ya no pueden recibir daño y resuelve un único impacto. Usa el mismo `BounceFromPogo` y luego aplica `TakeDamage`; el rebote ya queda resuelto aunque el daño desactive o destruya al enemigo. Los blancos sin `IDamageable` conservan su rebote sin daño.
- `Assets/_Project/Scripts/Player/PlayerPogoData.cs` y `Assets/_Project/Data/Player/PlayerPogoData.asset`: añaden `Damage`, inicialmente `1`, como parámetro de diseño. Conservan `BounceSpeed=13`, ventana `0.2 s`, offset `(0,-0.18)` y tamaño `(0.65,0.35)`.
- `Assets/_Project/Prefabs/Enemies/GroundEnemy.prefab`: añade el componente existente `PogoTarget` al objeto raíz. No cambia colliders, capas, Rigidbody2D, referencias ni parámetros anteriores. MovementTest hereda la integración del prefab sin modificar la escena.
- `Assets/_Project/Scripts/Editor/GroundEnemySceneSetup.cs`: añade la marca al crear futuros prefabs y verifica que el enemigo la tenga. La comprobación de los blancos antiguos usa sus dos nombres explícitos en lugar de exigir que solo existan dos `PogoTarget` en toda la escena.
- `Assets/_Project/Scripts/Editor/GroundEnemyValidation.cs`: incorpora la nueva regresión a la batería de Play Mode existente, después de las pruebas de colisiones/recuperación.
- `Assets/_Project/Scripts/Editor/GroundEnemyPogoValidation.cs` y su `.meta`: pruebas de integración con un teclado sintético del Input System y los componentes/consultas reales. Los fixtures se crean y eliminan únicamente en Play Mode. Las opciones temporales de foco de entrada se restauran al terminar.
- `Docs/DevelopmentLog.md`: registro de la causa, solución y pruebas. Se preservan todos los cambios previos sin commit. No hay commits ni push y no se implementa la Tarea 10.

### Comportamiento conservado

- La salamandra cae por gravedad natural. No se modifica `PlayerController`, no se añade velocidad descendente ni se cambia la fuerza de rebote, dash, doble salto, parry o cámara.
- El ataque descendente acertado causa `1` de daño inicial. `EnemyHealth` conserva su multiplicador x2 durante vulnerabilidad, por lo que ese mismo golpe causa `2` en dicho estado. Romper postura sigue produciendo aturdimiento/vulnerabilidad temporal, sin ejecuciones ni instakill.
- La ventana se cierra tras el primer impacto válido y recupera la disponibilidad del pogo como antes. Varios colliders del mismo enemigo no multiplican daño ni rebote. Los golpes fallidos no rebotan ni restituyen el uso antes de aterrizar/acertar, y los ataques horizontal/ascendente no activan la ventana descendente.
- Los dos objetivos dorados anteriores siguen aceptando el pogo. Se mantienen `EnemyBody`, exclusión de contacto entre cuerpos, colisiones con suelo y corrección de patrulla fuera de límites.

### Validación y preparación de pruebas

- Unity `6000.5.5f1` compiló y ejecutó la batería completa. La ejecución final de `Logs/Task09PogoValidation.log` terminó con `TASK09_POGO_PASS`, `TASK09_COLLISION_PASS`, `TASK09_RECOVERY_PASS` y `TASK09_PLAYMODE_PASS`, sin errores C#, avisos C# ni excepciones. Los tres impactos verificaron vida `4 → 3`, `4 → 2` vulnerable y `1 → 0` letal, conservando el rebote en todos los casos. `git diff --check` pasó.
- Se compararon por hash 12 archivos protegidos con su estado al iniciar esta integración: movimiento, salud, combate y parry del jugador; controlador/vida del enemigo; datos de movimiento/salud; Input Actions; ajustes físicos/capas y MovementTest. Todos se conservaron. La única modificación de datos del jugador corresponde al nuevo daño de pogo; sus parámetros de rebote y hitbox permanecen iguales.
- La regresión nueva usa S + J para pogo, D + J para ataque horizontal y W + J para ascendente mediante Input System. Comprueba daño y velocidad vertical reales, sin llamar directamente al rebote ni a la recepción de daño del jugador para simular un éxito.
- Cubre enemigo vivo, vulnerable y golpe letal normal; impacto único con varios colliders; fallo sin daño/rebote ni aceleración añadida; horizontal/ascendente sin rebote; y ambos blancos dorados. La vulnerabilidad y la vida previa al golpe letal se preparan mediante los contratos existentes, para aislar cada caso; la batería anterior comprueba también los parries y la muerte por combate.
- Se ejecuta la batería completa, incluidas las regresiones de colisiones sin empuje, recuperación de patrulla, combate, parry, postura, muerte y entrenador. Al comenzar, el Editor del usuario estaba abierto; por ello se utilizó una copia sincronizada del proyecto bajo `.utmp/Task09CollisionValidation`, ignorada por Git. No se controló ni cerró la sesión interactiva.
- En el primer intento se detectó una espera insuficiente en la preparación del caso fallido: tras reposicionar al personaje, la prueba enviaba el ataque antes de actualizar su estado de suelo. Se añadió esa espera solo en el fixture. La copia también carecía de `UserSettings/Search.index`, provocando una excepción interna del buscador de Unity; se copiaron sus archivos de configuración existentes sin alterar el proyecto principal ni suprimir errores.
- La preparación de los ataques normal/ascendente espera también a que termine `PlayerCombatData.AttackWindow` entre pulsaciones. No se redujo ni modificó la ventana del combate para encadenar artificialmente las pruebas.

### Pruebas manuales recomendadas

1. Abrir MovementTest y comprobar que el objeto raíz de GroundEnemy tiene `PogoTarget`, conserva su collider sólido en `EnemyBody` y mantiene sus datos anteriores. Entrar en Play Mode; no hace falta regenerar la escena.
2. Saltar sobre el enemigo vivo y pulsar abajo + J durante la caída. Debe rebotar hacia arriba y aplicar un punto de daño por golpe, usando la misma fuerza de los blancos dorados. Dejar caer al personaje sin atacar para comprobar que el contacto corporal por sí solo no produce rebote.
3. Acertar tres parries para romper postura y repetir el pogo durante el aturdimiento: debe rebotar y causar el daño vulnerable x2. Si el golpe agota la vida, debe completar la muerte y conservar el rebote.
4. En otra sesión de Play Mode, dejar al enemigo con una vida mediante tres ataques normales y terminar con pogo: debe morir mientras la salamandra sale hacia arriba. R sigue reiniciando solo al jugador; para restaurar al enemigo, salir y volver a entrar en Play Mode.
5. Usar abajo + J en el aire lejos de objetivos: caída natural, sin rebote, impulso descendente añadido ni daño remoto. Repetir ataques horizontal y ascendente contra el enemigo: deben causar su daño habitual sin rebote.
6. Repetir pogo sobre los dos blancos dorados. Comprobar también salto/doble salto, dash a través del enemigo sin empujarlo, patrulla/persecución, parry, vulnerabilidad, muerte y entrenador anterior; revisar la consola.
7. Desde Edit Mode se puede repetir la batería con `Fangbound/Validate Ground Enemy in Play Mode`. Los casos de pogo deben registrar `TASK09_POGO_CASE_PASS` y `TASK09_POGO_PASS`, además de las marcas de colisiones/recuperación y el resultado final general.

### Pendiente conservado

- Continúa pendiente la verificación manual específica de los `0.8 s` de i-frames de `PlayerHealth`. Esta integración no cambia vida, invulnerabilidad ni knockback del jugador.

## Tarea 10: Feedback de combate provisional

### Auditoría y puntos de confirmación

- Se leyeron `AGENTS.md`, `GDD.md`, este registro y `Region1Plan.md`, y se inspeccionaron combate, pogo, salud del jugador/enemigo, postura, parry, hitbox compartida y entrenador. El árbol de trabajo estaba limpio y HEAD era `aa44d6b` (Tarea 09 con pogo y correcciones); el usuario confirmó las tareas 01–09 probadas manualmente y con commit.
- No existía hitstop ni otro propietario de `Time.timeScale`. Sí existían destellos de `EnemyHealth`/`TrainingTarget`, feedback de éxito/fallo de `PlayerParry` y color vulnerable/de recuperación de `EnemyPosture`. Se reutilizaron sin reemplazar el combate.
- Un impacto normal se confirma dentro de `PlayerCombat.ApplyHitbox`, tras validar `IDamageable.CanReceiveDamage` y aplicar `TakeDamage`; su conjunto de objetivos ya evita daño repetido por varios colliders. El pogo confirma su impacto en `PlayerPogo`, después de reconocer `PogoTarget`, rebotar y aplicar daño. El éxito de parry se confirma exclusivamente en `PlayerParry.TryConsumeWindow`; `PlayerImpactReceiver` conserva su orden muerte → dash → i-frames → parry → daño. La ruptura se confirma cuando `EnemyPosture` pasa a cero.

### Arquitectura y archivos creados

- `Assets/_Project/Scripts/Combat/CombatFeedbackData.cs`: ScriptableObject con opciones de hitstop y feedback visual. `Assets/_Project/Data/Combat/CombatFeedbackData.asset` contiene los valores iniciales; no almacena estados de ejecución.
- `Assets/_Project/Scripts/Combat/CombatHitstop.cs`: servicio único por escena, con solicitudes normal/pogo/parry, reloj no escalado, combinación acotada de solicitudes y restauración del tiempo. Expone `IsActive`, `PauseCount` y `Cancel` para diagnóstico/validación.
- `Assets/_Project/Scripts/Combat/CombatVisualFeedback.cs`: coordinación pequeña de apariencia y señales. En enemigos gestiona el color del cuerpo y del indicador de postura; en el jugador gestiona únicamente un sprite hijo de éxito, manteniendo la propiedad del cuerpo en los sistemas anteriores. Se suscribe a daño confirmado, muerte, parry correcto y ruptura. Sus contadores sirven para comprobar efectos únicos.
- `Assets/_Project/Scripts/Editor/CombatFeedbackSceneSetup.cs`: menú `Fangbound/Configure Movement Test Combat Feedback`. Crea datos si faltan, actualiza el prefab mediante la API del Editor y enlaza jugador, entrenador y servicio en MovementTest, sin ejecutar los configuradores generales de movimiento/combate.
- `Assets/_Project/Scripts/Editor/CombatFeedbackValidation.cs`: menú `Fangbound/Validate Combat Feedback in Play Mode`, que ejecuta la batería anterior ampliada con regresiones específicas de feedback e interrupciones.
- Unity generó los `.meta` de scripts, datos y carpeta nueva. No se añadieron materiales, shaders, recursos externos, paquetes ni dependencias de pago; las señales reutilizan el sprite cuadrado y material existentes.

### Archivos existentes modificados

- `PlayerCombat.cs`: solicita un hitstop normal por ataque confirmado; un indicador temporal por ventana impide una segunda solicitud dentro del mismo ataque, además del filtrado de colliders existente.
- `PlayerPogo.cs`: solicita hitstop de pogo solo si el impacto válido tiene `IDamageable`. Mantiene rebote previo al daño, fuerza `13`, daño actual y caída natural; los blancos dorados sin daño no activan pausa.
- `PlayerParry.cs`: emite `Succeeded` y solicita hitstop tras consumir correctamente la ventana. Conserva lectura de inputs, colores anteriores, ventana `0.15 s` y recuperación `0.4 s`.
- `EnemyHealth.cs`: emite `DamageTaken` tras descontar vida y expone su color de muerte. Delega el color al coordinador cuando está instalado/activo, y conserva el destello anterior como alternativa si no lo está. Daño, multiplicador vulnerable, colliders, Rigidbody2D y muerte mantienen sus reglas.
- `EnemyPosture.cs`: emite `Broken` en la transición a cero y expone renderer/color derivados del estado existente. Delega únicamente la escritura visual cuando hay coordinador; conserva su alternativa anterior. No se modificaron la pérdida de postura, los `1.5 s` de vulnerabilidad ni la recuperación.
- `GroundEnemyValidation.cs` y `GroundEnemyPogoValidation.cs`: incorporan la batería nueva y verifican hitstop único en pogo vivo, vulnerable y letal, y ausencia de pausa al fallar.
- `GroundEnemy.prefab`: añade coordinador y señal visual hija. `MovementTest.unity`: añade esas referencias al jugador/entrenador y el objeto `Combat Hitstop`; la instancia terrestre hereda el feedback del prefab. Se conservaron los IDs/referencias de los componentes existentes.
- `Docs/DevelopmentLog.md`: documenta esta tarea. No se modificaron `PlayerController`, `PlayerHealth`, cámara, datos de movimiento/combate/parry/pogo/postura/vida, Input Actions, IA terrestre, hitbox compartida, física/capas, paquetes, GDD ni plan de región. No se realizaron commits, push ni cambios de rama.

### Hitstop y restauración

- Se usa `Time.timeScale=0` durante una micro-pausa porque permite detener juntos movimiento, gravedad, hitboxes y temporizadores de combate, sin alternar `Rigidbody2D.simulated`, cambiar colliders, reescribir motores ni guardar/restaurar velocidades. El rebote del pogo queda almacenado antes de la pausa y se conserva al reanudar.
- El servicio guarda la escala previa, utiliza `Time.realtimeSinceStartupAsDouble` para el vencimiento y no cambia `Time.fixedDeltaTime`. No depende de una coroutine escalada que pudiera quedar detenida con su propia pausa.
- Las solicitudes simultáneas comparten una única pausa: puede prevalecer la duración mayor, calculada desde el comienzo original, pero nunca se suman duraciones ni se crea una cola. Las duraciones se limitan a un máximo configurable en Inspector de `0.2 s`.
- Restaura al vencer, al desactivar/destruir el servicio, al perder foco o al recibir pausa de aplicación. También limpia la referencia estática al iniciar Play Mode sin recarga de dominio. No inicia hitstop si otro sistema ya tiene la escala a cero; si detecta un cambio externo a una escala distinta de cero, lo respeta.
- Los `Update` de entrada siguen ejecutándose. Los controles existentes almacenan salto/dash/pogo y las ventanas de ataque/parry permanecen en tiempo de simulación. Se verificaron pulsaciones breves de salto/dash durante la pausa, liberadas antes de reanudar.
- Todos los temporizadores escalados se detienen coherentemente con el mundo, incluidos parry, recuperación, vulnerabilidad e i-frames. No se cambiaron sus valores ni el código de salud. Una pausa añade tiempo real de presentación: `0.8 s` de simulación no equivale a `0.8 s` de reloj si interviene hitstop. La medición manual exacta pendiente de PlayerHealth debe hacerse también con hitstop desactivado para aislar el sistema original.
- Se desactiva globalmente desmarcando `Hitstop Enabled` en el asset; desactivar el componente `CombatHitstop` cancela inmediatamente una pausa activa. En escenas sin el servicio, las solicitudes son inocuas. No hay cambios globales persistentes al salir de Play Mode.

### Prioridades y efectos visuales

- Muerte tiene prioridad: cancela destello/señal y conserva el color final de cuerpo e indicador; el jugador conserva su manejo de muerte y el coordinador oculta su señal de éxito.
- Ruptura emite una señal dorada más grande y persistente que el parry. El indicador existente mantiene su turquesa durante toda la vulnerabilidad; el cuerpo terrestre recibe además un tinte derivado de ese estado.
- Un daño durante vulnerabilidad mezcla brevemente blanco sobre el tinte del cuerpo, mientras la franja turquesa conserva su identidad. Al terminar el destello vuelve al color derivado del estado actual, no a un color normal que borre la postura.
- El parry exitoso mantiene el feedback anterior del jugador y añade una señal cian breve detrás de su sprite. El fallo no emite `Succeeded`, no genera esa señal y no solicita hitstop.
- Las señales son sprites cuadrados provisionales, sin colliders, que crecen ligeramente y se desvanecen. Usan tiempo no escalado para seguir siendo visibles durante la micro-pausa. El color del cuerpo enemigo y el indicador tienen un único coordinador activo; los scripts anteriores solo escriben su alternativa cuando ese coordinador no está activo.
- Telegraph sigue en su renderer independiente y no se sobrescribe. El controlador existente lo cancela al morir o entrar en vulnerabilidad. El entrenador conserva sus ciclos y capacidades originales.

### Valores provisionales

| Parámetro | Valor inicial |
| --- | --- |
| Hitstop normal / pogo / parry | 0.05 / 0.04 / 0.07 s reales |
| Destello de daño | 0.10 s reales, blanco |
| Señal de parry | 0.16 s reales, cian, escala 1.35 |
| Señal inicial de ruptura | 0.25 s reales, dorado, escala 1.7 |
| Tinte vulnerable del cuerpo | Mezcla de 0.6 con el color de postura |
| Destello durante vulnerabilidad | Mezcla de 0.4 con blanco |
| Ventana de vulnerabilidad existente | 1.5 s de simulación, sin cambio |

### Validación realizada

- Unity `6000.5.5f1` compiló y ejecutó `CombatFeedbackSceneSetup.Configure`: `TASK10_SETUP_PASS` y salida `0` en `Logs/Task10Setup.log`.
- La batería completa se ejecutó en Play Mode del proyecto principal y terminó con `TASK10_VISUAL_PASS`, `TASK10_PARRY_PASS`, `TASK10_HITSTOP_PASS`, `TASK10_FEEDBACK_PASS` y `TASK09_PLAYMODE_PASS` en `Logs/Task10Validation.log`, sin errores C#, avisos C# ni excepciones.
- Se probaron ataque fallido sin pausa; horizontal y ascendente con pausa; un único impacto/destello con colliders duplicados; pogo vivo/vulnerable/letal con rebote conservado y pausa única; ruptura única; daño sin perder la franja vulnerable; apariencia tras recuperación; y muerte que no vuelve a colorearse por un efecto tardío.
- Se comprobó parry fallido sin éxito ni pausa, tres parries exitosos del entrenador sin duplicación ni daño, ruptura del entrenador y recuperación. La batería anterior sigue comprobando además parry/postura del enemigo terrestre, ataques, patrulla/persecución, bordes, muerte, colisiones sin empuje y los dos blancos antiguos de pogo.
- Se probaron veinte solicitudes solapadas sin acumular pausas, restauración automática a escala previa `0.5`, cancelación al desactivar/perder foco, respeto de escala externa `0.75`, opción desactivada y rechazo de una pausa externa a cero. Se verificaron inputs reales de salto/dash durante la pausa, recuperación posterior y congelación coherente de relojes de parry/i-frames. `fixedDeltaTime` permaneció igual.
- Las pruebas usan física/componentes reales y teclado sintético. Se preparan por reflexión algunas ventanas de parry/estados y una copia temporal de los datos de feedback para disponer de pausas largas en los casos de input; esos datos no se guardan. No se hizo una prueba humana ni se generó una build standalone.

### Pruebas manuales recomendadas

1. Abrir MovementTest en Unity `6000.5.5f1` y entrar en Play Mode. La escena y el prefab ya están configurados; no hace falta ejecutar los generadores anteriores.
2. Golpear con J horizontal y arriba + J: observar micro-pausa y destello del enemigo; fallar lejos de objetivos y comprobar que no hay pausa. Repetir con el enemigo moviéndose y revisar que sigue patrullando/persiguiendo/avisando al recuperarse.
3. Hacer abajo + J en el aire sobre el enemigo: comprobar daño, micro-pausa y rebote normal de fuerza 13, también en un golpe letal. Fallar y repetir sobre los blancos dorados: deben conservar su comportamiento anterior.
4. Usar K/hombro izquierdo justo antes del impacto del entrenador y del enemigo. El éxito debe producir la señal cian y pausa más marcada; el fallo conserva su color anterior y no genera señal de éxito. Acertar tres parries para observar la señal dorada de ruptura y el indicador turquesa durante la vulnerabilidad.
5. Golpear durante vulnerabilidad: el destello debe percibirse sin perder la franja turquesa; al terminar debe recuperar su apariencia actual. Matar durante el destello o ruptura: debe quedar con su color final, sin señal residual ni recoloreado posterior.
6. Probar movimiento, salto, doble salto, dash sin empuje, parry, muerte/R y controles de teclado/mando. Pulsar salto/dash durante una micro-pausa y comprobar la respuesta al reanudar. Revisar la consola y comprobar que nunca queda congelado.
7. Comparar con `Hitstop Enabled` desmarcado en `Assets/_Project/Data/Combat/CombatFeedbackData.asset`. Reponerlo al terminar; ajustar únicamente los parámetros de ese asset si la intensidad o duración requieren calibración. No cambiar todavía valores de combate/movimiento.
8. Para repetir las pruebas automáticas desde Edit Mode: `Fangbound/Validate Combat Feedback in Play Mode`. También el menú anterior de validación terrestre ejecuta ahora la batería ampliada.

### Limitaciones y pendientes

- Feedback provisional sin sonidos, partículas complejas, materiales/shaders definitivos, animaciones finales ni camera shake. La percepción visual y el balance deben revisarse manualmente; esta entrega no los certifica.
- El hitstop es global y detiene también el resto de la simulación de la escena. Si se añade posteriormente un gestor de menús/pausa que escriba simultáneamente el mismo valor cero, deberá coordinar la propiedad del tiempo mediante `Cancel`; dos escritores de cero no pueden distinguirse por leer `Time.timeScale`. Actualmente no existe ese segundo gestor.
- La señal inicial usa tiempo real y la vulnerabilidad usa tiempo de simulación. Las señales visuales no prolongan ni acortan la ventana de vulnerabilidad.
- Sigue pendiente la medición manual exacta de los `0.8 s` de i-frames de `PlayerHealth`. No se cerró ese pendiente ni se modificó su sistema para incorporar feedback.
- No se realizaron commits, push, cambios de rama ni trabajo de Tarea 11.

## Tarea 11: Llamas del Recuerdo — checkpoints y respawn

### Inspección y decisiones

- Se leyeron `AGENTS.md`, `GDD.md`, este registro y `Region1Plan.md`, y se revisaron salud/muerte/R, Input System, controlador, cámara, HUD, combate, hitstop, MovementTest y prefabs. HEAD era `2482378` (T10); el usuario confirmó T01–T10 terminadas, probadas manualmente y con commit.
- Existía un cambio local en `CombatFeedbackData.asset`: `normalHitstop=0.10` y `hitstopEnabled=0`. Se conserva exactamente; no se reactivó hitstop ni se modificaron otros parámetros de feedback en el asset.
- R ya llamaba a `PlayerHealth.RestartForTesting`, que restablece al jugador en la misma escena y no la recarga. No es necesario añadir persistencia estática del checkpoint, archivos, PlayerPrefs ni objetos persistentes entre escenas.
- La acción `Player/Interact` ya existía, con E y `<Gamepad>/buttonNorth`, y ningún componente la utilizaba. Se reutiliza esa acción sin editar Input Actions ni desplazar controles existentes. La lectura es por `WasPressedThisFrame`, sin repetición al mantener pulsado.
- La decisión narrativa/visual quedó registrada oficialmente en GDD, sección 5, subsección **Llamas del Recuerdo — descanso y checkpoints (decisión oficial)**. Se reemplazó la referencia genérica de calidad de vida por su nombre oficial, sin duplicar el diseño ni definir la causa narrativa de la reaparición.

### Arquitectura y archivos creados

- `Assets/_Project/Scripts/World/MemoryFlameData.cs` y `Assets/_Project/Data/World/MemoryFlameData.asset`: parámetros de interacción, colores, brillo provisional, señal de activación y comprobación del punto de reaparición. Los estados temporales permanecen en componentes.
- `Assets/_Project/Scripts/World/MemoryFlame.cs`: componente de la llama con rango, referencia a su hijo `Respawn Point`, renderers reemplazables y estado visual. Un registro de instancias activas permite buscar la llama más cercana dentro de su propio rango; se limpia al iniciar Play Mode sin recarga de dominio.
- `Assets/_Project/Scripts/Player/PlayerCheckpoint.cs`: lee Interact, muestra la indicación provisional de descanso, mantiene el checkpoint actual y emite `CheckpointChanged`/`Rested`. Rechaza descanso muerto, en dash, con control bloqueado, fuera de rango o con destino inseguro. Una interacción válida usa `TryHeal(MaximumHealth)`; no codifica el máximo 5 por separado. Reutilizar la llama actual cura/señala sin crear otro checkpoint ni emitir otro cambio.
- `Assets/_Project/Prefabs/World/MemoryFlame.prefab`: pedestal de piedra con tres sprites, llama flotante blanco-turquesa y brillo provisional. No tiene colliders, scripts de movimiento ni luces/partículas complejas; no cambia las colisiones del nivel. Los renderers y el punto de reaparición pueden reemplazarse/recolocarse sin reescribir la lógica.
- `Assets/_Project/Scripts/Editor/MemoryFlameSceneSetup.cs`: menú `Fangbound/Configure Movement Test Memory Flames`; crea datos/prefab si faltan, enlaza el componente del jugador e incorpora dos instancias sin regenerar la escena ni reconfigurar sistemas anteriores.
- `Assets/_Project/Scripts/Editor/MemoryFlameValidation.cs`: menú `Fangbound/Validate Memory Flames in Play Mode`; incorpora pruebas de E/R reales a la batería existente. Se añadieron sus `.meta` y los de carpetas/assets nuevos.

### Archivos existentes modificados

- `PlayerHealth.cs`: consulta opcionalmente el destino del checkpoint en el reinicio existente. Si no hay uno válido, conserva `spawnPosition`/`spawnRotation` anteriores. Desactiva brevemente combate/pogo/parry/controlador para limpiar sus ventanas mediante los `OnDisable` existentes, después restaura vida máxima, velocidad, gravedad, controles y estados como antes. Añade `Respawned` al terminar. No cambia recepción de daño, duración de i-frames, knockback, muerte ni HUD.
- `CameraFollow2D.cs`: añade `SnapToTarget`, que conserva el eje Z, respeta los límites actuales y limpia la velocidad de suavizado al teletransportar. `PlayerCheckpoint` lo llama al recibir `Respawned`, también para el spawn alternativo. No se modifican el seguimiento habitual, dead zone ni smooth time.
- `GroundEnemyValidation.cs`: integra la regresión de checkpoints. Para comprobar T10 aunque el diseñador haya desactivado hitstop, la batería usa una copia de sus datos en memoria con hitstop activo; mantiene las duraciones configuradas y restaura la referencia original al terminar o fallar. No guarda esa copia ni cambia el asset local.
- `MovementTest.unity`: añade `PlayerCheckpoint`, el grupo `Memory Flames` y dos instancias del prefab; conserva geometría, cámara, spawn inicial, jugador, objetivos, entrenador, enemigo y feedback anteriores.
- `Docs/GDD.md` y `Docs/DevelopmentLog.md`: decisión oficial y documentación de esta entrega. No se modifican Input Actions, parámetros de salud/movimiento/combate, datos de feedback, IA, física/capas ni el prefab enemigo. No hay commits, push, cambios de rama ni tareas posteriores.

### Descanso, estado y seguridad

- El checkpoint actual pertenece al componente del jugador durante la sesión. Solo la llama actual mantiene el brillo activo; al elegir otra, la anterior vuelve a tenue y puede volver a activarse. Cada descanso válido emite un destello blanco breve, también al reutilizar la misma llama.
- El descanso restaura vida hasta el máximo existente (`5` en MovementTest), sin superarlo. No teletransporta, reinicia enemigos, abre menús ni desbloquea habilidades. Tampoco cambia los temporizadores de daño/invulnerabilidad al curar.
- El punto configurado se valida al descansar y nuevamente antes de reaparecer. La comprobación usa el tamaño/offset real del collider del jugador, una holgura y la máscara de terreno: rechaza sólidos ocupando el cuerpo y exige soporte bajo los pies. Excluye el propio jugador y triggers; `EnemyBody` conserva su exclusión de contacto físico y no se convierte en suelo.
- Si se desactiva/destruye la llama, se ocupa su destino o deja de haber suelo, el reinicio usa el spawn original. No se busca una posición arbitraria ni se atraviesan paredes para corregir un destino mal configurado.
- No se recarga la escena al pulsar R, por lo que el checkpoint sobrevive a la muerte/reinicio. Salir de Play Mode o cargar otra escena/recrear al jugador inicia otra sesión; la persistencia entre niveles y el guardado permanente quedan fuera de esta entrega.

### Configuración y posiciones

| Parámetro | Valor inicial |
| --- | --- |
| Interacción | E / botón norte del mando, acción Interact existente |
| Rango | 1.3 unidades |
| Destello de activación | 0.30 s reales |
| Llama tenue | Turquesa apagado, alfa 0.4 |
| Llama actual | Blanco-turquesa, alfa 1 |
| Brillo tenue / activo | Alfa 0.08 / 0.30 |
| Holgura de respawn / sonda de suelo | 0.02 / 0.20 unidades |
| Desplazamiento local del Respawn Point | (-0.6, 0.58) |

- `Memory Flame - Start`: altar en `(-7.9, -2.1)`, destino aproximado `(-8.5, -1.52)` sobre el suelo inicial, a la izquierda de la zona de ataque del entrenador.
- `Memory Flame - Upper`: altar en `(5.8, 2.725)`, destino aproximado `(5.2, 3.305)` sobre la plataforma alta existente, fuera del alcance vertical del enemigo terrestre. Se llega con las habilidades de movimiento ya disponibles.

### Validación

- Unity `6000.5.5f1` compiló y ejecutó el configurador mediante la API del Editor: `TASK11_SETUP_PASS`, salida `0`, en `Logs/Task11Setup.log`.
- La ejecución final completa terminó con `TASK11_CHECKPOINT_PASS` y `TASK09_PLAYMODE_PASS` en `Logs/Task11Validation.log`, junto a las marcas de T10, sin errores C#, avisos C# ni excepciones. Incluye R con el cuerpo dinámico libre y desplazamiento horizontal real después de reaparecer, no solo la medición del destino con el fixture inmovilizado.
- La regresión usa teclado sintético para Interact/E y Restart/R. Comprueba ausencia de activación fuera de rango/muerto; primer descanso con curación; tecla mantenida sin repetición; reactivación sin duplicar checkpoint; sustitución por la segunda llama; persistencia con muerte/R; vuelta a la primera; colores/activación; curación limitada al máximo; referencias del HUD/cámara; y limpieza de ventanas de combate, bloqueo y knockback.
- También prueba que un destino ocupado se rechace y provoque fallback al spawn original, y que vuelva a ser válido al retirar el obstáculo. La batería anterior comprueba movimiento, dash sin empuje, pogo normal/vulnerable/letal, parry, postura, IA, muerte, hitstop y feedback.
- Las pruebas disponen temporalmente de cuerpo inmovilizado en los casos de interacción y preparan algunos estados por los contratos existentes/reflexión. No se modifican ni guardan parámetros del juego para simular un éxito. La revisión humana del aspecto y del recorrido sigue siendo necesaria; no se generó una build standalone.

### Pruebas manuales exactas en Unity

1. Abrir `Assets/_Project/Scenes/Test/MovementTest.unity` en Unity `6000.5.5f1`. No regenerar la escena completa. Entrar en Play Mode: ambas llamas comienzan tenues y no hay checkpoint actual.
2. Para comprobar la alternativa original, morir sin usar E y pulsar R: debe volver a `Player Spawn Point`, con vida completa y controles operativos.
3. Acercarse a la llama del suelo inicial, alrededor de `x=-7.9`. Debe aparecer la indicación de descanso con E y el binding del mando disponible. Pulsar E: señal blanca, brillo activo y vida completa. Para probar curación, recibir daño del entrenador, alejarse a la izquierda y descansar cuando termine el bloqueo de knockback.
4. Mantener E: no debe producir varios descansos. Soltar y volver a pulsar permite curarse otra vez. Alejarse más de `1.3` unidades: desaparece el aviso y E no activa la llama.
5. Usar salto/doble salto y las plataformas existentes para llegar a la llama de la plataforma alta, cerca de `(5.8, 2.725)`. Pulsar E: esta se ilumina y la primera vuelve a tenue.
6. Volver al combate, dejar que el enemigo o entrenador agote la vida y pulsar R. Debe reaparecer sobre la plataforma alta, aproximadamente en `(5.2, 3.305)`, con 5/5 de vida, sin knockback ni bloqueos. La cámara debe mostrarlo inmediatamente respetando sus límites; comprobar HUD, caminar, salto, ataque, parry, dash, doble salto y pogo.
7. Volver a la primera llama y pulsar E: al morir/R debe reaparecer junto a ella. Los enemigos muertos no resucitan y los vivos conservan su estado; no hay respawn global del mundo.
8. Para configurar otro destino, mover el hijo `Respawn Point` del prefab/instancia, manteniendo al menos la holgura sobre suelo libre. Activar Gizmos al seleccionar la llama muestra rango y referencia del cuerpo. Un punto dentro de un sólido o sin suelo será rechazado.
9. Desde Edit Mode, usar `Fangbound/Validate Memory Flames in Play Mode` para repetir la batería completa. Los menús anteriores de validación incluyen también el nuevo caso. Los ajustes locales de hitstop permanecen intactos al finalizar.

### Límites y pendiente conservado

- Representación de sprites provisionales, sin arte final, partículas, audio, menús, viaje rápido, recuerdos/cinemáticas ni explicación narrativa definitiva de la reaparición. Las variantes regionales pueden sustituir los hijos visuales conservando el componente.
- Comprobación conservadora mediante caja del cuerpo y máscara de terreno; para nuevos niveles se deben configurar correctamente las capas y el punto de spawn. No implementa búsqueda de rutas ni reparación automática de destinos.
- Sigue pendiente la medición manual exacta de los `0.8 s` de i-frames de PlayerHealth, fuera del alcance de T11. Sus valores no se alteraron.

## Ajuste final de T11: contraste del HUD de vida

- El usuario confirmó T11 aprobada en sus pruebas manuales. Se ajustó únicamente la presentación de `PlayerHealthUI.cs`: lleno `#59D9D0`, vacío `#34343D`, borde `#91A9AC` de 2 px, configurables mediante campos serializados del componente. Se conservan los cinco indicadores actuales, tamaño de 26 px, separación de 5 px y posición original.
- Los bloques usan `Texture2D.whiteTexture` con rellenos opacos, evitando que el fondo del skin de `GUI.Box` oscurezca los colores. El HUD sigue leyendo `CurrentHealth`/`MaximumHealth` en cada dibujo; no se modificaron salud, daño, invulnerabilidad, curación, muerte, respawn ni checkpoints. Se restaura `GUI.color` al terminar para no afectar otras indicaciones.
- En `CombatFeedbackData.asset` se activó `Hitstop Enabled`. Se conserva la duración normal ya configurada en `0.10 s`, pogo `0.04 s` y parry `0.07 s`, sin cambios en otros parámetros.
- Unity `6000.5.5f1` compiló y ejecutó la batería completa en una copia aislada para no cerrar el Editor del usuario. `Logs/Task11HudValidation.log` terminó con `TASK11_CHECKPOINT_PASS` y `TASK09_PLAYMODE_PASS`, incluidas las regresiones de T10, sin errores C#, avisos C# ni excepciones. Se cubren daño, curación/reactivación/cambio de llama, muerte y R; `git diff --check` pasó.
- El contraste calculado entre lleno/vacío es aproximadamente `7.2:1`; lleno/fondo de cámara `9.6:1` y borde/fondo `6.6:1`. El borde mantiene distinguibles los huecos oscuros sobre el fondo de la escena.
- Se revisaron capturas del Game View real y sus píxeles: `5/5`, `4/5`, `2/5` y `1/5` tienen exactamente los rellenos `#59D9D0`/`#34343D`; al curarse o cambiar checkpoint se conservan cinco llenos, al morir cinco vacíos y tras R cinco llenos. La comprobación gráfica aislada terminó con `HUD_RENDER_TRANSITIONS_PASS` en `Logs/Task11HudRender.log`; las imágenes están en `Logs/HudPreview-*.png`. La herramienta temporal de captura queda fuera de los archivos del producto, en la carpeta ignorada `.utmp`.
- Prueba manual: en MovementTest recibir daño para comparar `5/5`, `4/5`, `2/5` y `1/5`; descansar con E para recuperar los cinco bloques turquesa; cambiar de llama; morir para ver los cinco huecos y reaparecer con R para recuperar `5/5`. Comprobar también el hitstop activo en impactos normales. No se hicieron commits, push ni cambios de rama. La medición manual exacta de los `0.8 s` de i-frames continúa pendiente.
