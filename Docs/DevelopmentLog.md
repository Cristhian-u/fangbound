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