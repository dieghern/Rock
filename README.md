# Get This Rock Out of Here

Prototipo single-player para **Unity 6000.6.0f1** (URP + Input System).

## Empezar

1. **Detén Play Mode y vuelve a iniciarlo.** El proyecto usa GardenRock como escena inicial de Play. Si hace falta, abre `Assets/GetThisRock/Scenes/GardenRock.unity`.
2. El personaje levanta una tablet 3D. En **Contratos**, acepta **01 · Limpieza de jardín**.
3. Empiezas con las manos, $0, nivel 1 y cuatro espacios vacíos si no existe un guardado.
4. Acércate a cada roca pequeña, apunta hacia abajo y pulsa **E**. Cada roca ocupa un espacio y la seleccionada se muestra en las manos.
5. Lleva las dos rocas al rectángulo junto a **ENTREGA / SUELTA + ENTER**. Selecciona sus espacios con **1–4** y suéltalas con **G** dentro del rectángulo.
6. El HUD debe mostrar **2/2**. Pulsa **Enter** para recibir **$20 y 25 XP**. También funciona con la tablet abierta; allí hay un botón de cobro cuando todo está listo.
7. Repite el contrato inicial para llegar a nivel 2 y desbloquear el siguiente.

Una roca en el inventario no cuenta como entregada. Debe estar suelta y completamente dentro del volumen. Si sale antes de confirmar, deja de contar. El pago y la XP se otorgan una sola vez por trabajo aceptado.

## Controles

| Acción | Control |
| --- | --- |
| Mover / mirar / saltar | WASD / mouse / Espacio |
| Recoger roca pequeña | E |
| Seleccionar espacio de inventario | 1, 2, 3, 4 |
| Manos sin objeto | F o 0 |
| Soltar la roca seleccionada | G |
| Empujar roca mediana/grande | Manos seleccionadas, acercarse y mantener clic |
| Tablet | Tab |
| Inventario dentro de la tablet | I |
| Terminar contrato listo | Enter o Enter numérico |
| Palanca | Seleccionar su espacio, apuntar al borde inferior y mantener clic |
| Cuerda | Seleccionar su espacio, E para enganchar/desenganchar, retroceder o mantener clic para tensar |
| Liberar cursor / recuperar control | Esc / clic |

Los números ahora seleccionan **espacios**, no herramientas fijas. Las manos no consumen espacio. Las herramientas compradas ocupan un espacio cada una y no se descartan accidentalmente con G. Si tienes dos herramientas, quedan dos espacios para rocas; puedes hacer varios viajes.

La tablet bloquea la entrada del jugador mientras está abierta, conserva las rocas almacenadas y suelta la cuerda. La física del escenario continúa. Seleccionar otra ranura guarda la roca anterior y muestra la nueva; al soltar se reactiva el mismo Rigidbody, sin crear copias ni lanzar el objeto.

## Progresión

| Contrato | Nivel requerido | Rocas | Pago | Experiencia |
| --- | --- | --- | --- | --- |
| Limpieza de jardín | 1 | 2 pequeñas: 3 y 5 kg | $20 | 25 XP |
| Jardín revuelto | 2 | 3 pequeñas + 1 mediana de 55 kg | $55 | 45 XP |
| Limpieza de camino | 3 | 2 pequeñas + 2 medianas de 85/100 kg | $110 | 70 XP |
| Esto ya es trabajo | 4 | 1 pequeña + 1 mediana + 1 grande de 280 kg | $190 | 100 XP |

Umbrales acumulados: nivel 2 a **50 XP**, nivel 3 a **200 XP**, nivel 4 a **450 XP**. Los contratos desbloqueados pueden repetirse para ganar dinero y XP. Solo hay un contrato activo a la vez.

La **palanca cuesta $60** y la **cuerda $100**. No se descuenta dinero si no hay espacio, saldo o si ya posees la herramienta. Cada compra añade una mecánica física, no una mejora porcentual de fuerza.

## Guardado permanente

Dinero, XP y herramientas se guardan automáticamente al confirmar un trabajo y al comprar. Se restauran al iniciar el juego. Las rocas recogidas y el contrato en curso son temporales; salir de Play reinicia el trabajo pendiente, pero conserva el progreso cobrado.

Archivo: `Application.persistentDataPath/rock-profile-v1.json`. Se escribe primero un temporal y se reemplaza el perfil anterior, conservando una copia `.bak`. Un error de lectura/escritura se informa en la tablet. Las pruebas usan un archivo separado y no modifican el perfil del jugador.

## Assets y animación

- `Assets/GetThisRock/Art/RockCoTablet.prefab`: modelo 3D propio con carcasa redondeada, pantalla, cámara, altavoces, botón y puerto; sus mallas y materiales están en la misma carpeta.
- `Assets/GetThisRock/Art/WorkGloveHand.prefab`: manos enguantadas estilizadas, con palmas, dedos y puños.
- `FirstPersonPresentation`: anima levantar/bajar la tablet, acercar la vista, sujetarla, extender las manos al empujar y colocarlas alrededor de una roca recogida. Son animaciones procedurales de prototipo, sin assets externos ni animaciones compradas.
- La interfaz interactiva se ajusta a la proyección de la pantalla de la tablet.

## Organización

| Archivo/carpeta | Función |
| --- | --- |
| `Scripts/Inventory/PlayerInventory.cs` | Cuatro espacios compartidos, selección, herramientas y referencias de rocas |
| `Scripts/Progression/PlayerProgression.cs` | XP y cálculo de nivel |
| `Scripts/Progression/GameSave.cs` | Persistencia de dinero, XP y herramientas |
| `Scripts/Progression/GameContent.cs` | Catálogo central y conexión de componentes al iniciar |
| `Resources/RockContent.asset` | Referencias de contratos, herramientas y prefabs visuales |
| `Scripts/Contracts/Contract.cs` | Definición data-driven con lista de rocas, nivel, pago y XP |
| `Scripts/Contracts/ContractManager.cs` | Spawn de todas las rocas, contador de entrega y transacción de cobro |
| `Scripts/Contracts/DeliveryZone.cs` | Contención completa de colliders |
| `Scripts/Equipment/PlayerEquipment.cs` | Compras permanentes que ocupan inventario |
| `Scripts/Equipment/RockToolUse.cs` | Palanca y cuerda físicas |
| `Scripts/Interaction/PlayerInteraction.cs` | Recoger, guardar, soltar y empujar |
| `Scripts/Presentation/FirstPersonPresentation.cs` | Presentación y animaciones en primera persona |
| `Scripts/UI/GameHUD.cs` | Tablet, lista desplazable de contratos, tienda, inventario y HUD |
| `Editor/PresentationAssets.cs` | Generación reproducible de prefabs y mallas propias |
| `Editor/ProgressionSetup.cs` | Configuración de escena y contenido; escena inicial de Play |
| `Editor/GardenRockValidation.cs` | Pruebas de inventario, entrega, XP, compras y guardado |
| `Editor/ProgressionPlayValidation.cs` | Pruebas en Play Mode con perfil y dispositivos virtuales |

`SessionInstaller` conecta las referencias al iniciar incluso si una escena antigua carece de los nuevos componentes. El HUD muestra un mensaje de configuración en lugar de romper toda la interfaz con una referencia nula. La validación de entrega ya no depende de un único identificador de roca ni de estar mirando en una dirección específica para pulsar Enter.

## Editar y comprobar

Los contratos están en `Assets/GetThisRock/Data/`: modifica masas, tamaños, posiciones, pago, XP y nivel en Inspector. Añade contratos al catálogo `Resources/RockContent.asset`. Los precios viven en `Lever.asset` y `Rope.asset`.

**Get This Rock > Validate Milestone** prueba cuatro espacios, recogida/soltado, entrega de varias rocas, pago único, XP por repetición, niveles, compras con inventario lleno y guardado/recarga. Reabre la escena para descartar mutaciones de prueba. Resultado: `Logs/GardenRockValidation.txt`.

**Get This Rock > Upgrade Contracts and Shop** regenera los valores iniciales de contenido y conexiones. Guarda primero cambios propios que quieras conservar. Las pruebas de Play Mode se ejecutan exclusivamente en la copia bajo `Logs/ValidationProject`; su comando cierra ese editor al finalizar.

## Próxima etapa

Un contrato con rocas parcialmente enterradas para dar más utilidad a la palanca, seguido de persistencia del contrato en curso si se necesita reanudar trabajos a mitad de camino.

## Relieve, rocas y carretilla

- Las rocas usan una malla de granito erosionado más densa y una colisión convexa irregular, también en los fragmentos.
- Al empujar, las palmas se orientan hacia la superficie; brazos y dedos acompañan la aproximación, la presión y la retirada.
- Compra y selecciona la carretilla. **E** la despliega o la apoya en sus patas. Permanece en el mundo al seleccionar otra herramienta o abrir la tablet.
- Con la carretilla apoyada, pulsa **F**, recoge piedras pequeñas con **E** y suéltalas dentro de la bandeja con **G**. Vuelve a seleccionar la carretilla, acércate a los mangos y pulsa **E** para transportarla con WASD. La carga tiene física libre: puede caer si vuelcas la carretilla. Para descargar, recoge las piedras de la bandeja y suéltalas en ENTREGA.
- Los contratos generan desniveles con colisión; las colinas tienen mayor relieve. La carretera y el área de entrega mantienen una superficie nivelada.
- Validación adicional: `GetThisRock.Editor.RealismValidation.Run` comprueba el relieve, la colisión de las rocas y el transporte físico de carga. Resultado en `Logs/RealismValidation.txt`.
