# SuperMarioClone

Un progetto per imparare Godot costruendo insieme un platform 2D, con l'obiettivo futuro di giocare in due online dal browser.

## Base attuale

Una sola scena, `main.tscn`, senza script:

- `Player`: un personaggio fermo con forma e collisione.
- `Ground`: il terreno.
- `Platform`: una piattaforma.
- Il titolo e una scritta per riconoscere il prototipo.

Il progetto usa il renderer Compatibility e un'area di gioco di 1280 x 720. Movimento, salto, modalità di gioco e multiplayer sono ancora da implementare. L'esportazione Web non è ancora configurata o verificata.

Apri `project.godot` con Godot 4.7.x standard e premi **F5** per vedere la scena. Usate entrambi la stessa versione di Godot e condividete il progetto con Git.

## Regole concordate

- **Cooperativo:** entrambi devono arrivare al traguardo; se uno muore, perdono entrambi.
- **Competitivo:** vince il primo al traguardo; chi muore riparte dal proprio ultimo checkpoint raggiunto, oppure dall'inizio.
- La modalità si sceglierà prima della partita.

## Prossimo esercizio

Progettate il movimento orizzontale: decidete i comandi, la velocità e cosa succede quando si rilascia un tasto. Scrivete queste tre decisioni prima di creare lo script.

La scena è un esperimento: potete modificarla o eliminarla quando deciderete come organizzare il gioco.
