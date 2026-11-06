# FANGBOUND — GDD v2 (unificado)

*Metroidvania 2D de combate exigente · Unity · Proyecto en solitario Cris · 28 oct 2026 · Entrega: 18 dic 2026 (build listo el 15 dic)*

> **Actualización de alcance (5 de noviembre de 2026):** Arboleda Verdescente se planificará con **15–18 salas (objetivo inicial: 16)**. Esta decisión reemplaza únicamente el tamaño anterior de la región 1. El cronograma y las referencias a regiones de 4–6 salas que aparecen más abajo pertenecen al GDD original y deberán revisarse antes de comprometer fechas.

---

## 1. Resumen en 30 segundos

Una salamandra despierta herida en una arboleda, con una espada corta hecha del colmillo de una bestia ancestral. Esa bestia fue su maestro: le enseñó que la fuerza sirve para proteger a quien no puede defenderse. Ahora, convencido de que la libertad es la causa del dolor, el maestro quiere activar un mecanismo antiguo, el Velo del Silencio, que apagaría el sufrimiento del mundo a costa de la voluntad de todos.

Tema: proteger a alguien no significa decidir por esa persona. Frase conceptual: «Atado a un pasado del que no puede escapar.»

Idea que une historia y juego: el parry es responder, no dominar. El jugador gana leyendo al enemigo y respondiendo a su ritmo; la bestia pierde porque quiso controlarlo todo. La mecánica estrella dice el tema sin necesidad de diálogo.

Promesa de la demo (lo que sí se entrega): 3 regiones cortas, 3 bosses, un parry impecable, 3 escenas de memoria y un cierre que deja ver el Velo. Las regiones 4 a 8 quedan como visión: mapa completo, arte conceptual y esta historia ya escrita.

## 2. Decisiones ya tomadas

| Tema | Decisión |
| --- | --- |
| Nombre | FANGBOUND («Colmillo de Salamandra» queda como nombre interno viejo) |
| Equipo | Solo, con IA como apoyo |
| Formato de presentación | Demo jugable; un video corto de respaldo por si falla algo |
| Arte | Sprites 2D (no pixel art). Bocetos tuyos → IA los refina → limpieza y animación en Unity |
| Presupuesto | Cero: solo herramientas gratuitas |
| Controlador del jugador | Propio y simple (ver sección 11) |
| Historia | Versión FANGBOUND (maestro y aprendiz + Velo del Silencio) |
| Final | «Lo siento…» (propuesto; ver sección 14) |

## 3. Historia

### 3.1 La salamandra (protagonista)

Criatura antropomórfica de piel oscura con manchas amarillas o naranjas que emiten luz. Capa gastada de viaje. Una cicatriz profunda le recorre el cuerpo hasta la cola; casi siempre queda oculta, solo se ve un fragmento en la cola.

Es perseverante y observadora. No es una víctima sin identidad: su forma de luchar (leer, esperar, responder) ya dice quién es. Su diferencia clave con el maestro: entiende que proteger no da derecho a decidir por el otro.

Arma: la Daga del Gran Colmillo, un objeto sencillo, no una reliquia legendaria. Es el vínculo con el maestro.

### 3.2 El maestro (la bestia)

Enorme criatura de aspecto cocodriliano que fue un protector respetado. Su principio original: quien tiene fuerza debe usarla para ayudar a quien no puede defenderse.

Su caída es gradual: primero decidió por otros para evitarles peligros, luego les impidió decidir lo que juzgaba dañino, y al final concluyó que toda libertad capaz de causar dolor debe restringirse. No se cree malvado. Cree que el mundo se lo agradecerá. Su tragedia: sigue queriendo proteger, pero olvidó que los protegidos tienen derecho a elegir.

### 3.3 Qué pasó antes del juego

1. Maestro y aprendiz tuvieron una relación real: respeto, confianza y afecto, no solo obediencia.
2. En unas ruinas, el maestro descubrió la filosofía de una civilización desaparecida que prometía un mundo sin sufrimiento, y empezó a aplicarla de forma absoluta.
3. La salamandra intentó detenerlo. En la pelea le arrancó un colmillo, pero no pudo frenarlo y quedó gravemente herida (la cicatriz).
4. El juego empieza cuando despierta. No perdió todos sus recuerdos: recuerda enseñanzas y sensaciones, pero no los detalles de la pelea ni por qué fracasó. El misterio es qué pasó y hasta dónde llegará el maestro, no quién es ella.
Tres símbolos visuales: el colmillo (el vínculo), la cicatriz (el precio de enfrentarlo) y la capa (oculta el pasado y simplifica la silueta).

### 3.4 El plan: el Velo del Silencio

La civilización antigua intentó alterar mente y emociones mediante energía que viaja por cristales. No logró paz perfecta: la gente seguía queriendo cosas distintas y eso generó nuevos conflictos. El maestro concluye que, mientras las personas puedan desear, perder y equivocarse, el sufrimiento no termina. Decide completar lo que la civilización no pudo.

El Velo haría tres cosas:

- Eliminar el sufrimiento extremo: el duelo, el miedo y el odio perderían casi toda su intensidad.
- Suprimir los impulsos que generan conflicto: la rebeldía y los deseos «peligrosos» se debilitarían.
- Imponer una calma permanente: la gente sería feliz, pero sin haber elegido lo que es ni lo que ama.
No son esclavos encadenados: son seres a quienes se les quitó la capacidad de imaginar otra vida. Ese es el horror, y por eso el conflicto funciona. La bestia lo ve como la salvación.

Lo que piensa (no hace falta decirlo literal): «¿De qué sirve la libertad si permite que alguien destruya su vida? ¿De qué sirve amar si puede convertirse en dolor?» Su error es confundir eliminar el sufrimiento con eliminar lo que da sentido a una vida.

### 3.5 El final

Tras el último golpe, la bestia cae, la música baja casi al silencio y la salamandra no celebra. Baja el colmillo y se queda junto a su maestro. Con la voz rota, él dice solo:

> **«Lo siento…»**

Ella no responde ni lo perdona. Pasan unos segundos de silencio y se marcha. Sin discurso, sin sonrisa de la bestia, sin primer plano dramático; la emoción sale del contraste entre lo que fueron y lo que ella tuvo que hacer. El mecanismo queda detenido y el mundo conserva su capacidad de elegir, con sus riesgos y su dolor.

La idea final: no es que la paz sea mala; es que una paz impuesta a costa de la voluntad de todos deja de ser protección.

### 3.6 Reglas de presentación de la historia

- Mostrar primero las consecuencias de la bestia y después sus motivos.
- Mostrar su bondad antes de revelar su corrupción.
- La civilización antigua no es «malvada»: quería terminar con el dolor. El problema es el método.
- Los habitantes no opinan todos igual de la bestia (algunos le agradecen, otros la temen).
- El centro emocional es la relación maestro–aprendiz.
- Cada recuerdo = una ilustración fija + una línea de texto + música. Sin animación ni diálogos largos.
- La historia principal se entiende sin recoger secretos; los recuerdos opcionales solo profundizan.

## 4. La historia por regiones

Cada región enseña una skill, revela una parte de la historia y muestra un pedazo del sistema del Velo. Así el final no introduce nada nuevo.

| # | Región | Función narrativa | Recuerdo / qué ve el jugador | Pista del Velo | Skill |
| --- | --- | --- | --- | --- | --- |
| 1 | Arboleda Verdescente | Presentar a la salamandra y un pasado desconocido | Una enseñanza del maestro, sin decir quién es; criaturas huidizas y marcas de fuerza descomunal | Cristales lila apagados en el suelo; criaturas raramente calmadas | Dash, doble salto |
| 2 | Abismo Lagrimal | Primeras consecuencias de la bestia | La bestia defendiendo a un indefenso; algunos la recuerdan como protectora, otros solo conocen el miedo | Habitantes que perdieron las ganas de irse | Cañón de agua, daga de hielo, flash |
| 3 | Caldera Ígnea | Humanizar al maestro | Maestro y aprendiz juntos: «Ser fuerte no significa que puedas obligar a los demás a obedecerte. Significa que puedes protegerlos cuando ya no tienen fuerzas para protegerse a sí mismos.» | Un nodo de cristal dormido en la lava | Amuleto térmico, Colmillo Ígneo |
| 4 | Dominio Amatista | La investigación de la civilización antigua | La bestia estudiando inscripciones (orden, sacrificio, fin del sufrimiento) | Los cristales transmiten y transforman energía | Escudo de cristal (parry mejorado) |
| 5 | Cámara Dorada | Por qué la filosofía era atractiva | La bestia leyendo los textos con esperanza, no con maldad | Los registros: querían terminar con guerras y hambre | Mejora de daño |
| 6 | Ciudad Olvidada | El fracaso original | La bestia entiende las consecuencias y decide aplicarlas con más fuerza | Espacios hechos para vigilar y limitar | Llave de la región 7 |
| 7 | Ciénaga Venenosa | El coste real del plan | Comunidades con opiniones distintas; la bestia toma una decisión irreversible, con calma | Se activan los nodos del Velo | Dagas venenosas, antídoto |
| 8 | Umbría Umbral | Resolver maestro vs. aprendiz | La verdad de la pelea original; el origen del colmillo y la cicatriz | El mecanismo a punto de activarse | Prueba de todo lo aprendido |

Cómo cierra la demo (regiones 1 a 3): el último boss es un duelo dentro de un recuerdo (ver sección 7). Al ganarlo, aparece la frase del maestro, el nodo de cristal en la lava se enciende un instante y una silueta enorme se aleja en la distancia. Pantalla final: «Continuará… FANGBOUND» y el mapa completo de las 8 regiones. Así la demo tiene un arco cerrado y deja ver la visión.

## 5. Mecánicas

### Movimiento

Correr, salto, dash con invulnerabilidad breve y doble salto (se desbloquean en la región 1). Ataque descendente con rebote (pogo): al golpear hacia abajo suelo, pinchos o enemigos, la salamandra sale impulsada hacia arriba.

### Barra de postura (aturdimiento)

Jugador, enemigos y bosses tienen una. Al llenarse, su dueño queda aturdido y vulnerable a un golpe crítico.

| Acción | Barra del jugador | Barra del enemigo |
| --- | --- | --- |
| Recibir un golpe | Sube | Sin cambio |
| Defender sin parry perfecto | Sube (menos que un golpe) | Sin cambio |
| Parry perfecto | No sube | Sube mucho |
| Golpear al enemigo | Sin cambio | Sube poco a poco |
| Barra llena | Aturdido y vulnerable a crítico | Ejecución / crítico |

La barra del jugador baja sola despacio tras unos 3 s sin recibir daño, para no castigar de más.

### Parry

Botón dedicado. Ventana inicial sugerida: 0,15 s, ajustable desde un ScriptableObject. Debe sentirse impecable: pausa de impacto (hitstop), destello en las manchas, temblor de pantalla corto y un sonido propio. Es lo que el jurado recordará.

### Ataques no parryables

Todo enemigo tiene ataques parryables y no parryables. Los no parryables se avisan con un destello rojo y obligan a esquivar; si te dan, es golpe crítico.

### Energía de skills

Una barra que se recarga con parry perfecto y golpes. El combate defensivo alimenta el ofensivo.

### Filosofía de dificultad

- Parry puro: la ruta más exigente.
- Habilidades y combos: crean ventanas, reducen defensas o interrumpen ataques, pero no vuelven triviales a los bosses (congelar dura poco, el veneno es limitado, ciertas fases resisten).
- Dominio avanzado: parry + habilidades = ganar más rápido y con más eficiencia.

### Calidad de vida (cuentan mucho en un juego difícil)

- Checkpoint visible antes de cada boss y reinicio rápido de la pelea.
- Selector de dificultad con modo «historia» para la presentación.
- Zona extra post-juego (arena de bosses): solo como visión.

## 6. Habilidades y combos

| Skill | Efecto | Combo | Región | Prioridad |
| --- | --- | --- | --- | --- |
| Dash | Desplazamiento rápido, invulnerabilidad breve | Base de movimiento | 1 | Núcleo |
| Doble salto | Segundo salto en el aire | Base de movimiento | 1 | Núcleo |
| Cañón de agua | Proyectil que empuja y activa mecanismos | + daga de hielo: congela unos segundos | 2 | Núcleo |
| Colmillo Ígneo (espada de fuego) | Imbuye el arma: más daño y quemadura; gasta energía, no es permanente | + aceite: quemadura prolongada | 3 | Núcleo |
| Daga de hielo | Marca y enfría al enemigo | + cañón de agua: congelación breve | 2 | Ampliación |
| Flash | Aturde 1 s a un enemigo | Abre ventana de golpe | 2 | Ampliación |
| Aceite | Zona resbaladiza e inflamable | + fuego: quemadura que baja la vida | 2 o 3 | Ampliación |
| Escudo de cristal | Parry especial: el parry perfecto daña y sube mucho la postura del atacante | Potencia el parry | 4 | Visión |
| Dagas venenosas | 5 dagas; el veneno baja la vida y reduce resistencias del jefe | Potencia parry y daño | 7 | Visión |

Amuleto térmico (región 3): se consigue en la primera mitad de la región, protege del calor ambiental y desbloquea el Colmillo Ígneo. La región enseña la skill y su boss la evalúa. Sin temporizadores que castiguen sin aportar.

Región 2: la lluvia es mecánica real, no decorado: apaga el fuego y refuerza el cañón de agua.

Nombres: se usa daga de hielo (encaja con el Abismo Lagrimal) y Escudo de cristal queda para el Dominio Amatista.

## 7. Bosses

Regla: todo boss tiene al menos 2 fases; los más duros, 3. Cada uno tiene también su barra de postura.

| Región | Boss | Fases | Mecánica especial | Prioridad |
| --- | --- | --- | --- | --- |
| 1 | Guardián Raíz (tronco vivo con raíces) | 2 | Enseña parry y esquiva de ataques no parryables | Núcleo |
| 2 | Hidra de la Lluvia (serpiente de agua que se divide en charcos) | 2 | La lluvia cambia sus patrones; se congela con daga + cañón | Núcleo |
| 3 | Eco del Maestro (duelo dentro de un recuerdo de entrenamiento) | 3 | Usa los movimientos que le enseñó a la salamandra; ataques de fuego no parryables | Núcleo |
| 3 (extra) | Capataz de Magma | 2 | Se carga de fuego; se apaga con agua para aturdirlo | Ampliación |
| 4 | Arconte de Amatista | 3 | Refleja proyectiles; hay que usar prismas | Visión |
| 5 | Rey Avaro | 3 | Monedas parryables, lingotes no parryables | Visión |
| 6 | Autómata Antiguo | 3 | Cambia las reglas de la sala a media pelea | Visión |
| 7 | Madre de la Ciénaga + jefe secreto | 3 | Veneno acumulativo con indicador claro (la piel se pone verde) y antídotos en el camino | Visión |
| 8 | La bestia (maestro) | 3 | Copia técnicas que enseñó; es la pelea más personal | Visión |

Cambio importante respecto al guion anterior: el «Cocodrilo de Brasas» de la región 3 chocaba con la nueva historia, porque la bestia real es el jefe final de la región 8. Se reemplaza por el Eco del Maestro: un duelo dentro de un recuerdo donde la salamandra revive un entrenamiento. Es perfecto para la demo: humaniza al maestro, enseña el parry con él como profesor y reutiliza los mismos movimientos en el jefe final (cuesta poco, se siente enorme).

Plantilla compartida: todos los bosses usan la misma máquina de estados (espera → ataque parryable → ataque no parryable → aturdido → cambio de fase). El primer boss cuesta el doble; los demás casi la mitad.

## 8. Alcance por niveles

| Nivel | Contenido | Estado objetivo el 15 dic |
| --- | --- | --- |
| Núcleo | Movimiento, combate, parry, postura; regiones 1 a 3 (cortas); bosses 1, 2 y 3 (Eco del Maestro); dash, doble salto, cañón de agua, Colmillo Ígneo; 3 memorias; menú y final de demo | Terminado y pulido |
| Ampliación | Capataz de Magma, daga de hielo, flash, aceite, música extra | Solo si el cronograma va a tiempo |
| Visión | Regiones 4 a 8, bosses 4 a 8, Escudo de cristal, dagas venenosas, arena de bosses | Documentado + arte conceptual |

Cada región del núcleo debe ser corta: 4 a 6 salas, un tramo de exploración, un checkpoint, un boss. Un buen combate en 3 zonas vale más que un mundo largo con errores.

## 9. Arte, audio y herramientas (presupuesto cero)

### Arte 2D de sprites

Flujo recomendado: boceto tuyo → IA lo refina con tu boceto como referencia → limpieza → separar por partes (brazo, torso, cola, cabeza, capa) → animación por huesos en Unity.

El riesgo real es la coherencia: la IA tiende a cambiar el diseño en cada imagen. Por eso lo primero es una hoja de personaje (vista de frente, perfil, manchas, capa, colmillo) que usarás como referencia en todo lo demás.

- Animación mínima: salamandra con inactivo, correr, salto, dash, ataque, parry, golpeada y morir. Cada boss con 4 animaciones (espera, ataque, aturdido, morir).
- Fondos: 3 capas de paralaje por región y una paleta por región (verde, azul, naranja; después lila, dorado, gris, verde tóxico, negro).
- Memorias: ilustraciones fijas, sin animación.
- Regiones 4 a 8: solo un concept art por región.

### Audio

- Música: una pista por región del núcleo y una de boss (con variante en la fase 2 si sobra tiempo). Opciones: Suno o Udio (revisa sus términos antes de publicar) o música libre de derechos (Pixabay Music, OpenGameArt, Incompetech).
- Efectos: Freesound o generadores gratuitos. Prioridad absoluta: parry perfecto, golpe de daga, dash y aviso de ataque no parryable.
- Regla: un buen sonido de parry vale más que una pista extra de música.

### Herramientas gratuitas

| Herramienta | Para qué |
| --- | --- |
| Unity (una sola versión, sin actualizar) | Motor |
| URP + luces 2D | Brillo en fuego y cristales, luz en las manchas |
| Input System | Teclado y mando con respuesta precisa para el parry |
| Cinemachine | Cámara, límites por sala, temblor |
| 2D Animation, Sprite Atlas, Tilemap | Huesos, atlas y niveles con tiles |
| TextMeshPro, DOTween (gratis), uGUI | HUD, barras y menús |
| Git | Volver atrás si la IA rompe algo |
| Krita (o tu programa de dibujo) | Bocetos y limpieza |

Sin presupuesto: no uses Feel ni Corgi Engine. El hitstop (pausar un instante) y el temblor de pantalla se programan en pocas líneas con ayuda de la IA, y el controlador propio te da control total del tiempo del parry.

Cómo usar la IA de código sin perder tiempo: dale este GDD como contexto al inicio de cada sesión, pide un sistema por vez, pide que todos los valores (daño, vida, ventana de parry) vivan en ScriptableObjects, pega siempre el error exacto de la consola de Unity y haz commit antes de cada cambio grande.

## 10. Cronograma (28 oct – 15 dic)

Siete semanas. El contenido se congela el 7 de diciembre. El 16 y 17 son colchón y el 18 es la entrega.

| Semana | Fechas | Entregable | Punto de control |
| --- | --- | --- | --- |
| 1 | 28 oct – 3 nov | Proyecto + Git, controlador (correr, salto, dash, doble salto, pogo), cámara, personaje con cajas, hoja de personaje terminada | Se mueve bien en una sala de prueba |
| 2 | 4 – 10 nov | Combate, parry, postura, un enemigo de prueba, hitstop y temblor | El parry perfecto se siente bien; si no, no se avanza |
| 3 | 11 – 17 nov | Región 1 completa, enemigos básicos, boss 1, HUD base, primera pasada de arte | Región 1 jugable de inicio a fin |
| 4 | 18 – 24 nov | Región 2, lluvia, cañón de agua, boss 2 | Dos regiones y dos bosses jugables |
| 5 | 25 nov – 1 dic | Región 3, amuleto, Colmillo Ígneo, Eco del Maestro, 3 memorias | Núcleo completo |
| 6 | 2 – 7 dic | Música, sonidos, menús, final de demo, concept art de regiones 4 a 8, ampliación si hay tiempo | Congelación de contenido el 7 dic |
| 7 | 8 – 15 dic | Solo errores, balance, build probado en otro equipo, video, guion de presentación | Build entregable el 15 dic |

Reglas de corte:

- Si el 17 de nov la región 1 y el boss 1 no son jugables, se elimina toda la ampliación y la región 3 se reduce a un tramo corto + el boss.
- Si el 24 de nov la región 2 no está jugable, la región 2 se reduce a un tramo corto y se prioriza el Eco del Maestro.
- Si el 1 de dic el núcleo no está completo, la semana 6 es solo para terminarlo.
- Nunca se retrasa una mecánica por música o arte; el audio y las ilustraciones se reemplazan al final.
Rutina diaria: 1) probar el juego 15 minutos, 2) un sistema nuevo con la IA, 3) commit al terminar, 4) anotar el siguiente paso para mañana.

## 11. Cómo empezar: los primeros 3 días

Sobre el «controlador del jugador» (tu duda): es solo el código que hace que el personaje corra, salte y haga dash. Es como la dirección y los pedales de un auto: no es el auto completo, pero sin eso no hay nada que probar. Recomendación: escribe uno propio y simple con ayuda de la IA (1 o 2 días), en lugar de aprender el código de un asset ajeno. Así el parry usa tu propio reloj y puedes ajustarlo con precisión.

### Día 1 (hoy)

1. Crea el proyecto 2D (URP) en la versión de Unity que ya tengas instalada y no la actualices.
2. Inicia Git y haz el primer commit.
3. Crea una sala de prueba: un suelo, dos plataformas y una pared con cajas.
4. Pídele a tu IA el controlador básico: correr y salto, con valores en un ScriptableObject.

### Día 2

1. Añade dash y doble salto.
2. Añade el ataque descendente con rebote (pogo).
3. Configura Cinemachine con la cámara siguiendo al jugador.

### Día 3

1. Haz tus bocetos de la salamandra (frente, perfil, capa, colmillo).
2. Pásalos por la IA para la hoja de personaje y elige una versión definitiva: no la cambies más.
3. Cambia las cajas por el sprite provisional.

### Prompt inicial para tu IA de código (pégalo con este GDD):

«Estoy haciendo un metroidvania 2D en Unity (C#). Necesito un controlador de jugador simple y modular: correr, salto, dash con invulnerabilidad breve, doble salto y ataque descendente con rebote. Todos los valores deben estar en un ScriptableObject para ajustarlos sin tocar código. Comenta el código por zonas y separa las funciones. Dame un sistema por vez y avísame cómo probarlo en una sala de prueba.»

## 12. Presentación del 18 de diciembre

- Formato: demo en vivo y un video de 60 a 90 segundos de respaldo (el mejor parry y el mejor boss).
- Guion de 10 minutos: 1 min de premisa (tema y frase conceptual), 7 min jugando la demo en modo historia, 2 min de visión (mapa de 8 regiones, concept art, el Velo del Silencio).
- Antes del día: probar la build en otro equipo, tener la build + el repositorio + el video en un pendrive y en la nube.
- Qué mostrar de tu trabajo con IA: indica de forma transparente qué partes usaron IA (arte base, música, código de apoyo) y qué fue tuyo (diseño, bocetos, balance, dirección).

## 13. Riesgos

| Riesgo | Probabilidad | Qué hacer |
| --- | --- | --- |
| Alcance demasiado grande para un solo desarrollador | Alta | Núcleo de 3 regiones cortas; el resto queda como visión |
| El arte y la animación consumen más que el código | Alta | Pocas animaciones, hoja de personaje fija, animación por huesos |
| La IA cambia el diseño entre imágenes | Alta | Una sola hoja de personaje como referencia siempre |
| El parry no se siente justo | Media | Probarlo desde la semana 2; ventana ajustable; avisos claros |
| Bosses demasiado difíciles frustran al jurado | Alta | Modo historia y checkpoint antes de cada boss |
| La IA genera código que rompe lo anterior | Media | Un sistema por vez, error exacto, commits frecuentes |
| Licencias de música e imágenes generadas | Media | Revisar términos antes de publicar; para una presentación basta con indicar el uso de IA |

Evita: multijugador, mundo procedural e inventario complejo.

## 14. Decisiones abiertas

- ☐ Final: «Lo siento…» (propuesto) o «Gracias…». Con el Velo del Silencio, «Lo siento» funciona mejor: pide perdón por lo que habría hecho con todos, no solo con ella.
- ☐ ¿Ponerle nombre al maestro? Un nombre propio aumenta el peso emocional; es opcional.
- ☐ Versión de Unity: confirmar cuál tienes instalada el día 1 y fijarla.
- ☐ Ampliación: decidir el 1 de dic si entra el Capataz de Magma o la daga de hielo + flash.
