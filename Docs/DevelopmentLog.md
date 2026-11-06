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
