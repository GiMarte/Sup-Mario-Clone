# SuperMarioClone

Un progetto per imparare Godot costruendo insieme un platform 2D, con l'obiettivo futuro di giocare in due online dal browser.

## Base attuale

La scena principale è `Scenes/Level1.tscn`: un livello a scorrimento ispirato al mondo 1-1. Il vecchio esperimento resta disponibile in `main.tscn`.

`Scripts/Level1.cs` gestisce camera, monete, interfaccia, ripartenza dalle cadute e arrivo alla bandiera. Terreno, tubi, blocchi e decorazioni sono nodi modificabili nella scena, costruiti con gli asset inclusi e regioni della mappa di riferimento. I movimenti e i premi riutilizzano questi script C#:

- `Scripts/Player.cs`: movimento con A/D, salto con Spazio, trasformazione dei vestiti e crescita da 32 a 40 pixel quando si raccoglie un fungo. **F** lancia palline del potere attivo.
- `Scripts/Block.cs`: `Hit()` trasforma la texture domanda in `UsedTexture` una sola volta e genera `RewardScene`.
- `Scripts/Mushroom.cs`: movimento e raccolta dei funghi, con `PowerType.Ice` o `PowerType.Fire`.
- `Scripts/Coin.cs`: giro della moneta e rimozione al termine dell'animazione.
- `Scripts/BlockCheck.cs`: controllo di proporzioni, colpi dal basso, texture e contatti laterali.

Il progetto usa il renderer Compatibility e un'area di gioco di 1280 x 720. Le modalità in due e il multiplayer sono ancora da implementare. L'esportazione Web non è ancora configurata o verificata.

Apri `project.godot` con Godot 4.7.2 .NET e premi **F5** per avviare il primo livello. **A/D** muovono il personaggio, **Spazio** salta, **F** lancia e **R** ricomincia. Le monete raccolte restano conteggiate durante la stessa partita; una caduta riporta alla partenza, ripristina il tempo e fa perdere il potere. Alla bandiera il personaggio raggiunge il castello e appare il risultato. Usate entrambi la stessa versione di Godot e condividete il progetto con Git.


Il fungo fuoco dà vestiti rosso/arancio e palline che eliminano i polipetti; il fungo ghiaccio dà vestiti azzurri e palline che li congelano per tre secondi. I colpi seguono la direzione del personaggio e rimbalzano sul terreno. La crescita mantiene i piedi sul pavimento e attende spazio libero se c’è un soffitto basso.

Le scene dei premi sono `Scenes/IceMushroom.tscn`, `Scenes/FireMushroom.tscn` e `Scenes/Coin.tscn`; quella del blocco è `Scenes/Block.tscn`. In `main.tscn`, `Block`, `Block2` e `Block3` mantengono rispettivamente fungo fuoco, fungo ghiaccio e moneta. I disegni conservano i loro nomi originali nella cartella `Assets`.

Per verificare il livello, apri `Scenes/Level1Check.tscn` e premi **F6**: il controllo verifica terreno, salti, premi, monete, cadute e attraversa automaticamente tutto il percorso fino alla bandiera.

Per verificare i poteri, apri `Scenes/PowerCheck.tscn` e premi **F6**: controlla raccolta, colori, crescita, tiro, congelamento, collisioni e ripartenza.

Per eseguire il controllo dei blocchi del prototipo, apri `Scenes/BlockCheck.tscn` e premi **F6**: il risultato compare nell'Output e il controllo termina da solo.

## Regole concordate

- **Cooperativo:** entrambi devono arrivare al traguardo; se uno muore, perdono entrambi.
- **Competitivo:** vince il primo al traguardo; chi muore riparte dal proprio ultimo checkpoint raggiunto, oppure dall'inizio.
- La modalità si sceglierà prima della partita.

## Prossimo esercizio

Seguite il percorso `Block.Hit()` → `Mushroom.Activate()` → `Player.ReceivePower()` → `PowerBall` per studiare come raccolta, trasformazione e tiro comunicano tra loro.

`main.tscn` resta un esperimento separato. Il primo livello usa il giocatore singolo attuale; le modalità in due descritte sopra restano da implementare.
