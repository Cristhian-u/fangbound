# Propuesta inicial: Arboleda Verdescente

## Enfoque

Arboleda Verdescente presenta a la salamandra herida en un entorno que parece natural, pero conserva señales discretas de una antigua intervención sobre sus habitantes. La ruta principal enseña el espacio y el regreso por caminos conocidos; dos ramales opcionales ofrecen secretos y atajos. Dash y doble salto se obtienen en momentos distintos y abren conexiones previamente visibles. Los cristales lila están apagados: sus vetas, criaturas extrañamente tranquilas y restos de mecanismos insinúan el Velo del Silencio sin explicar todavía su origen.

El recorrido está pensado como una red de 16 salas, no como una secuencia cerrada. Los nombres son de trabajo. Las conexiones indican puertas, pasajes o cambios de altura previstos; no implican que las salas estén construidas.

## Salas y conexiones

| # | Sala | Propósito | Conexiones previstas |
| --- | --- | --- | --- |
| 1 | Claro del Despertar | Introducir el despertar, el punto de partida y las marcas de una criatura enorme. Un cristal lila apagado aparece entre raíces. | 2 |
| 2 | Sendero de Esporas | Primer espacio de orientación con dos salidas visibles; establece el retorno hacia el claro. | 1, 3, 4 |
| 3 | Pasarela Quebrada | Mostrar un hueco demasiado ancho para cruzarlo al inicio; al otro lado se ve una ruta útil, no un callejón arbitrario. | 2, 7 (requiere dash desde 6) |
| 4 | Hondonada de Helechos | Ruta lateral segura que recompensa explorar antes de conocer habilidades; oculta una entrada hacia el santuario. | 2, 5 |
| 5 | Santuario del Colmillo | Encontrar la primera memoria del maestro: una ilustración fija y una frase sobre observar antes de actuar. Incluye cristales apagados incrustados en piedra. | 4, 6, 12 (atajo de retorno desbloqueado más tarde) |
| 6 | Raíces Resonantes | Recibir dash en un punto claro y seguro; el diseño permite volver enseguida a la pasarela y probarlo. | 5, 3 |
| 7 | Tronco Partido | Llegada al ramal que antes se veía desde la pasarela; enseña que el dash también sirve para mantener rutas alternativas abiertas. | 3, 8, 9 |
| 8 | Galería de Musgo | Ramal opcional corto con un secreto ambiental y una pista de criaturas calmadas cerca de vetas lilas. | 7 |
| 9 | Nido Suspendido | Presentar alturas y plataformas que anticipan el doble salto sin exigirlo todavía en la ruta principal. | 7, 10 |
| 10 | Cámara del Polen Quieto | Espacio de lectura ambiental: polvo suspendido y fauna inmóvil alrededor de un cristal apagado; acceso a un secreto vertical posterior. | 9, 11, 13 (secreto, requiere doble salto) |
| 11 | Copa del Vigía | Desbloquear el doble salto tras una prueba breve de plataformas; conecta de vuelta con espacios conocidos. | 10, 12 |
| 12 | Atajo de las Raíces | Abrir un atajo entre el santuario y la zona avanzada, reduciendo el retroceso tras caer o volver a explorar. | 11, 5 (atajo), 13 |
| 13 | Umbral del Guardián | Preparar el primer combate con señales legibles del Guardián Raíz y sin añadir sistemas nuevos de forma inesperada. Contiene un escondite opcional accesible con doble salto. | 12, 10 (secreto), 14 |
| 14 | Refugio de Savia | Checkpoint antes del jefe, con espacio despejado y salida de retorno hacia el atajo. | 13, 12 |
| 15 | Cámara del Guardián Raíz | Combate contra el Guardián Raíz; el encuentro presenta lectura de ataques y respuesta precisa según el diseño general del juego. | 14, 16 (tras el encuentro) |
| 16 | Salida del Velo | Cierre de región: cristales lila apagados, una última pista de la calma impuesta y transición hacia Abismo Lagrimal. | 15, Abismo Lagrimal |

## Lectura del flujo

- **Inicio y exploración:** 1 → 2, con una bifurcación temprana entre 3 y 4. La pasarela de 3 enseña una posibilidad futura; la ruta 4 → 5 ofrece una alternativa exploratoria.
- **Dash:** 5 → 6 desbloquea dash; regresar por 5 → 4 → 2 permite tomar la pasarela 3 → 7. El atajo 5 ↔ 12 se abre desde el lado avanzado.
- **Doble salto:** desde 7 y 9 se alcanza 10 → 11; allí se obtiene doble salto. Se habilitan el secreto 10 → 13 y el paso elevado de 12.
- **Jefe y salida:** 12 → 13 → 14 establece el checkpoint; 15 alberga al Guardián Raíz y 16 conecta con Abismo Lagrimal.
- **Memoria y Velo:** la primera memoria está en 5 y es opcional, pero queda en un ramal temprano. Los cristales lila apagados y la calma inusual aparecen en 1, 5, 8, 10 y 16 como pistas, no como explicación completa.

## Decisiones para revisar antes de implementar

- Ajustar distancias y altura de los bloqueos al controlador real cuando exista.
- Definir si el acceso al atajo 5 ↔ 12 es un mecanismo, una puerta o una conexión física por raíces.
- Confirmar la ubicación definitiva del desbloqueo de cada habilidad y la duración de la memoria.
- Mantener la región entre 15 y 18 salas; esta propuesta fija el objetivo actual en 16.