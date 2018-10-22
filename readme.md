# Dépanneur Sigmund

[![Build Status](https://dev.azure.com/sigmundftw/Depanneur/_apis/build/status/Depanneur)](https://dev.azure.com/sigmundftw/Depanneur/_build/latest?definitionId=73)

Gestion des produits, des achats et des paiements pour le dépanneur de Sigmund.

## Pré-requis

- Visual Studio 2017 (version RC utilisée pour démarrer le projet)
  - Note: Il est aussi possible d'utiliser VS Code, mais la configuration de ce dernier est laissée comme exercice au lecteur.
- NodeJS (la version 6.9.4 a été utilisée pour le setup du projet)

## Installation développeur

- Clonez le repository
- Installez les packages: `npm install`
- Startez webpack pour builder le bundle frontend: `npm run webpack-watch`
- Ouvrez le projet dans Visual Studio 2017
- Faites un clic droit sur le projet _Depanneur.App_ et sélectionnez _Manage User Secrets_.
- Entrez vos identifiants pour l'authentification Google et pour votre compte SendGrid. Voir détails plus bas.
  ```json
  {
    "GoogleAuth": {
      "ClientID": "YOUR_CLIENT_ID",
      "ClientSecret": "YOUR_CLIENT_SECRET"
    },
    "Email": {
      "Mode": "SendGrid",
      "SendGrid": {
        "ApiKey": "YOUR_API_KEY"
      }
    }
  }
  ```
- Démarrez le projet (_Debug > Start Without Debugging_)
- Accédez à `http://localhost:59792/account/login`
- Cliquez sur le poisson et identifiez vous avec votre adresse Sigmund.

## Paramètres de configuration

- `GoogleAuth:ClientID` et `GoogleAuth:ClientSecret`  
  Identifiants OAuth pour une application Google. Celle-ci doit autoriser les redirect URIs suivants pour supporter les différents scénarios d'exécution possibles :
  - http://localhost:5000/signin-google
  - http://localhost:59792/signin-google
  - http://localhost:59793/signin-google
  - Votre serveur de production
- `GraphQL:EnableGraphiQL`  
  Mettre à `true` pour activer l'interface GraphiQL à l'URL `/graphql`. Utile pour tester des requêtes lors du développement.
- `GraphQL:ExposeExceptions`  
  Indique si le détail des exceptions se produisant lors d'une requête GraphQL doit être retourné. Utile lors du développement.
- `ForceSSL`  
  Mettre à `true` pour rediriger tout le traffic vers le protocol https.
- `Email:BaseUrl`  
  Spécifie le nom de domaine de l'application, utilisé dans les courriels.
- `Email:Mode`  
  Indique de quel façon les courriels doivent être envoyés. Valeurs possibles :
  - `SendGrid` : Pour envoyer les courriels via l'API SendGrid.
  - `Smtp` : Pour envoyer les courriels via le protocol SMTP.
- `Email:SendGrid:ApiKey`  
  Votre clé d'API pour SendGrid.
- `Email:Smtp:Host` et `Email:Smtp:Port`  
  Adresse et port de votre serveur SMTP.
- `DefaultTimeZone`  
  Votre fuseau horaire. Doit correspondre à un fuseau "standard" de votre plateforme. Par exemple :
  - Sous Windows : `Eastern Standard Time`
  - Sous Linux : `America/Toronto`
- `WhitelistedDomains`  
  Liste des noms de domaines dont les courriels sont automatiquement approuvés lors de la connexion. Séparez plusieurs domaines par un espace.

## Overview techno

### Backend

- ASP.NET Core
- ASP.NET Core Identity, configuré avec Google Authentication
- Entity Framework Core
- GraphQL
- Hangfire

### Frontend

- Webpack
- Babel avec preset ES2015
- React
- Radium (librairie react pour ajouter des fonctionnalités aux inline styles)
- Apollo
