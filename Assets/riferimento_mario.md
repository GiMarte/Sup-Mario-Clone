# Riferimenti grafici del pavimento e del cielo

Il pavimento attuale usa `CuboTerra.png`, estratto dal livello del file `CuboTerra.pixil` fornito dall'utente, senza modificare i pixel. Il PNG conserva la tela originale di 100 x 100 pixel.

In `main.tscn`, il nodo `Ground/Visual` usa la regione `(43, 28, 16, 16)`, che contiene soltanto il mattoncino. La scala `2 x 2` lo mostra a 32 x 32 pixel; le modalita `Tile` lo ripetono. Il filtro `Nearest` mantiene i pixel nitidi.

Il cielo usa `#5C94FC`, campionato dalla mappa originale del livello 1-1 pubblicata da Rick N. Bruns su [NESMaps](https://nesmaps.com/maps/SuperMarioBrothers/SuperMarioBrosWorld1-1MapBG.html). La mappa e conservata senza modifiche in `mario_world1_1_reference.png`. Grafica del gioco originale: Nintendo.
