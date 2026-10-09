# SuperMarioClone

Un progetto per imparare Godot costruendo insieme un platform 2D, con l'obiettivo futuro di giocare in due online dal browser.

## Base attuale

La scena principale è `main.tscn`, con questi script C#:

- `Scripts/Player.cs`: movimento con A/D, salto con Spazio e potere raccolto in `CurrentPower`.
- `Scripts/Block.cs`: `Hit()` trasforma la texture domanda in `UsedTexture` una sola volta e genera `RewardScene`.
- `Scripts/Mushroom.cs`: movimento e raccolta dei funghi, con `PowerType.Ice` o `PowerType.Fire`.
- `Scripts/Coin.cs`: giro della moneta e rimozione al termine dell'animazione.
- `Scripts/BlockCheck.cs`: controllo di proporzioni, colpi dal basso, texture e contatti laterali.

Il progetto usa il renderer Compatibility e un'area di gioco di 1280 x 720. Modalità di gioco, effetti dei poteri e multiplayer sono ancora da implementare. L'esportazione Web non è ancora configurata o verificata.

Apri `project.godot` con Godot 4.7.2 .NET e premi **F5** per avviare il gioco. Usate entrambi la stessa versione di Godot e condividete il progetto con Git.

Le scene dei premi sono `Scenes/IceMushroom.tscn`, `Scenes/FireMushroom.tscn` e `Scenes/Coin.tscn`; quella del blocco è `Scenes/Block.tscn`. In `main.tscn`, `Block`, `Block2` e `Block3` mantengono rispettivamente fungo fuoco, fungo ghiaccio e moneta. I disegni conservano i loro nomi originali nella cartella `Assets`.

Per eseguire il controllo dei blocchi, apri `Scenes/BlockCheck.tscn` e premi **F6**: il risultato compare nell'Output e il controllo termina da solo.

## Regole concordate

- **Cooperativo:** entrambi devono arrivare al traguardo; se uno muore, perdono entrambi.
- **Competitivo:** vince il primo al traguardo; chi muore riparte dal proprio ultimo checkpoint raggiunto, oppure dall'inizio.
- La modalità si sceglierà prima della partita.

## Prossimo esercizio

Leggete `Block.Hit()` e seguite il collegamento a `Mushroom.Activate()`. Progettate poi insieme l'effetto di uno dei poteri prima di scriverne il codice.

La scena è un esperimento: potete modificarla o eliminarla quando deciderete come organizzare il gioco.
