# StockFlow

**StockFlow** est une application de bureau de gestion de stock conçue pour les petites entreprises, commerces et indépendants. Développée en **C#**, avec **.NET 10**, **Avalonia UI** et **SQLite**, elle permet de suivre les produits et les mouvements de stock depuis Windows, Linux ou macOS (selon les environnements Avalonia disponibles).

> Langues de l'interface : français et anglais.

## Fonctionnalités

- **Tableau de bord** : nombre de produits, unités disponibles, valeur estimée du stock et nombre d'alertes.
- **Catalogue produits** : ajouter, modifier, rechercher et supprimer des produits.
- **Informations produit** : nom, référence SKU, catégorie, quantité, seuil minimum et prix unitaire.
- **Entrées de stock** : enregistrer les réceptions et augmentations de quantité.
- **Sorties de stock** : enregistrer les ventes et utilisations ; une sortie supérieure au stock disponible est refusée.
- **Alertes de stock bas** : repérer les produits dont la quantité est inférieure ou égale au seuil défini.
- **Rapport CSV** : exporter le catalogue pour l'ouvrir dans Excel ou un tableur compatible.
- **Stockage local** : les données sont conservées dans une base SQLite sur l'ordinateur.

## Technologies

| Technologie | Utilisation |
|---|---|
| C# / .NET 10 | Langage et plateforme |
| Avalonia UI 11 | Interface de bureau multiplateforme |
| SQLite / Microsoft.Data.Sqlite | Stockage local |
| XAML | Définition de la fenêtre |

## Prérequis

- .NET 10 SDK
- Un environnement compatible avec Avalonia UI
- Git (pour cloner le dépôt)

## Installation et lancement

1. Cloner le dépôt :

   ```bash
   git clone https://github.com/ChrisBOUONGOU/StockFlow.git
   cd StockFlow
   ```

2. Restaurer les dépendances :

   ```bash
   dotnet restore
   ```

3. Lancer l'application :

   ```bash
   dotnet run
   ```

Pour compiler une version Release :

```bash
dotnet publish -c Release
```

## Données locales

La base de données `stockflow.db` est créée automatiquement au premier lancement, dans le dossier de données locales de l'utilisateur :

- **Windows** : `%LOCALAPPDATA%\\StockFlow\\stockflow.db`
- **Linux** : `~/.local/share/StockFlow/stockflow.db` (selon l'environnement .NET)
- **macOS** : dossier Application Support associé à l'utilisateur (selon l'environnement .NET)

Pensez à sauvegarder ce fichier pour conserver vos données.

## Utilisation rapide

1. Ouvrir l'onglet **Produits** et créer un produit avec une référence SKU unique.
2. Définir la quantité initiale, le seuil minimum et le prix unitaire.
3. Utiliser l'onglet **Mouvements** pour enregistrer les entrées et sorties.
4. Consulter le **Tableau de bord** pour surveiller les niveaux bas.
5. Exporter le catalogue depuis **Rapports** au format CSV.

## Structure du projet

```text
StockFlow/
├── App.axaml
├── App.axaml.cs
├── MainWindow.axaml
├── MainWindow.axaml.cs
├── Program.cs
├── StockFlow.csproj
├── StockFlow.slnx
├── README.md
└── README.en.md
```

## Périmètre actuel

Cette version constitue une première base fonctionnelle : gestion du catalogue, ajustements de stock, alertes et export CSV. Les fonctionnalités suivantes pourront être ajoutées dans les prochaines versions : historique consultable et filtrable des mouvements, gestion des fournisseurs et clients, authentification multi-utilisateur, sauvegardes automatisées, impression de rapports et tests automatisés.

## Contribution

Les suggestions et contributions sont les bienvenues. Ouvrez une *issue* pour signaler un problème ou proposer une amélioration.

## Licence

Aucune licence open source n'est actuellement définie dans ce dépôt. Contactez l'auteur avant toute réutilisation ou redistribution.

---

Développé par **ChrisBOUONGOU** · [Dépôt GitHub](https://github.com/ChrisBOUONGOU/StockFlow)
