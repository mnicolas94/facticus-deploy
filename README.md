## ⚠️Notice: This project and its documentation are a work in progress. Use with care.

# Deploy

Deploy is a Unity package that allows you to build your game for multiple platforms and deploy them to various stores, remotely (rather than on your PC) and with a single click inside Unity. It uses [GitHub Actions](https://github.com/features/actions) and [GameCI](https://game.ci/) under the hood to run the builds on GitHub servers.

## Table of contents
- [Features](https://github.com/mnicolas94/facticus-deploy#features)  
- [Requirements](https://github.com/mnicolas94/facticus-deploy#requirements)  
- [How to install](https://github.com/mnicolas94/facticus-deploy#how-to-install)  
- [How to setup](https://github.com/mnicolas94/facticus-deploy#how-to-setup)  
    - [Create the workflow](https://github.com/mnicolas94/facticus-deploy#create-the-workflow)  
    - [Configure Secrets](https://github.com/mnicolas94/facticus-deploy#configure-secrets)  
    - [Secrets security](https://github.com/mnicolas94/facticus-deploy#secrets-security)  
- [How to use](https://github.com/mnicolas94/facticus-deploy#how-to-use)  
- [Documentation](https://github.com/mnicolas94/facticus-deploy#documentation)
- [Known issues](https://github.com/mnicolas94/facticus-deploy#known-issues)  
- [Acknowledgements](https://github.com/mnicolas94/facticus-deploy#acknowledgements)  

## Features
1. Platforms supported to build:
    - Windows
    - Linux
    - MacOS
    - WebGL
    - Android
2. Platforms supported to deploy:
    - Telegram
    - Itch.io
    - Play Store (Google Play)
3. [Override variables](https://github.com/mnicolas94/facticus-deploy/wiki/Override-variables-feature)

## Requirements
1. Your project has to be hosted on GitHub.
2. I only tested it on 2021.1+ Unity versions. Versions prior to that one should work, but I'm not sure.
3. > TODO
   
## How to install
### Via OpenUPM
To install it with [OpenUPM](https://openupm.com/) run:
`openupm add com.facticus.deploy`

### Via Git URL
Install dependencies first
- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/mackysoft/Unity-SerializeReferenceExtensions.git?path=Assets/MackySoft/MackySoft.SerializeReferenceExtensions`
- click <kbd>Add</kbd>

Then `https://github.com/mnicolas94/UnityUtils.git` with the same method.
And finally, install the Deploy package itself `https://github.com/mnicolas94/facticus-deploy.git`.

## How to setup
### Create the workflow
1. Inside Unity open the Deploy's editor window with `Tools/Facticus/Deploy/Open Deploy editor window`.
![facticus-deploy - SampleScene - Windows, Mac, Linux - Unity 2021 3 15f1 Personal DX11](https://github.com/mnicolas94/facticus-deploy/assets/35781652/92d9f9fa-c696-4255-ab33-e8b60de8cc8b)
2. [optional] Dock the window somewhere within your editor. This is recommended due to a known issue where the window wont open anymore if you lose its focus while it is floating.
3. Click on the `Package settings` tab.![facticus-deploy - SampleScene - Windows, Mac, Linux - Unity 2021 3 15f1 Personal DX11_2](https://github.com/mnicolas94/facticus-deploy/assets/35781652/a34f5e87-9cba-4096-b2c1-c5163a14f709)
4. First configure the `Git Directory` field if needed. It should point to your git repository's root directory relative to your Unity project's root. Commonly, both directories match, in which case you should leave the field empty.
5. Then, click the `Generate workflow` button and choose whatever name you want. This will be the name of the GitHub Actions workflow yml file created to build and deploy the project. Just keep this in mind to select a name different from other existent workflows you already have in your project.
6. Leave the other fields as they are for now and commit and push your changes. This will push the created workflow to GitHub in order for it to be called later.

### Configure Secrets
You need to configure some [Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets) in your Github repository in order to use this package. To add secrets you have to go to your repository's `Settings -> Secrets and variables -> Actions`. Almost each build and deploy platform has its own set of secrets that need to be configured in order to use it. You only need to configure the secrets of platforms you intend to use.

#### General secrets (required for any build and deploy platform)
- UNITY_LICENSE: The contents of a Unity license file (Unity_v20XX.x.ulf). Info on how to get it [here](https://game.ci/docs/github/activation).
- UNITY_EMAIL: The email address that you use to login to Unity.
- UNITY_PASSWORD: The password that you use to login to Unity.

Deploy does not acquire nor store your Unity email, password, or license file contents. They are required by [GameCI](https://game.ci/) to activate the license during build. More info [here](https://game.ci/docs/github/activation).

#### Build platforms' secrets
- Android:
    - ANDROID_KEYSTORE_NAME: Your Android keystore name. More info [here](https://game.ci/docs/github/builder#androidkeystorename).
    - ANDROID_KEYSTORE_BASE64: The base64 encoded contents of the android keystore file. You can obtain it by running `base64 $androidKeystoreName` where `androidKeystoreName` is the path to your keystore file. More info [here](https://game.ci/docs/github/builder#androidkeystorebase64).
    - ANDROID_KEYSTORE_PASS: The keystore password. More info [here](https://game.ci/docs/github/builder#androidkeystorepass).
    - ANDROID_KEYALIAS_NAME: The keystore alias name. More info [here](https://game.ci/docs/github/builder#androidkeyaliasname).
    - ANDROID_KEYALIAS_PASS: The keystore alias password. More info [here](https://game.ci/docs/github/builder#androidkeyaliaspass).
- Other platforms do not need to configure secrets.

#### Deploy platforms' secrets
- Telegram: for Telegram, you need to [create a Telegram bot](https://core.telegram.org/bots/features#creating-a-new-bot). You only need to create it, no coding is required.
    - TELEGRAM_SESSION: can be any string you like, e.g. "deploy". Once used do not change it.
    - TELEGRAM_API_ID: id of your telegram app. Visit https://my.telegram.org/apps to get one. More info in [Pyrogram setup](https://docs.pyrogram.org/start/setup). Only needed if your build exceeds 50 MB of size.
    - TELEGRAM_API_HASH: hash of yout telegram app. Visit https://my.telegram.org/apps to get it. More info in [Pyrogram setup](https://docs.pyrogram.org/start/setup). Only needed if your build exceeds 50 MB of size.
    - TELEGRAM_TOKEN: the token of your bot. You should have got this when created the bot.
    - TELEGRAM_CHAT_ID: the chat ID you want to send the build to. This [StackOverflow answer](https://stackoverflow.com/questions/32423837/telegram-bot-how-to-get-a-group-chat-id/32572159#32572159) describes how to get it;
- Itch
    - ITCH_BUTLER_CREDENTIALS: The key used by butler to authenticate. To get your Butler credentials, you can follow the [CI Builds Credentials documentation on Itch.io](https://itch.io/docs/butler/login.html#running-butler-from-ci-builds-travis-ci-gitlab-ci-etc)
    - ITCH_GAME: The logical name the game you want to push to. Eg. If your URL is `https://username.itch.io/example-project`, your value for `ITCH_GAME` would be `example-project`.
    - ITCH_USER: The username of the owner of the game you are pushing to. If your URL is `https://username.itch.io/example-project`, your value for `ITCH_USER` would be `username`.
- Play Store
    - PLAY_STORE_SERVICE_ACCOUNT_JSON: The contents of your `service-account.json` file. Follow this [instructions](https://developers.google.com/workspace/guides/create-credentials#service-account) on how to get it.
    - PLAY_STORE_PACKAGE_NAME: The package name, or Application Id, of the app you are uploading, e.g. `com.example.myapp`.

#### Notification platforms' secrets
A notification platform is a channel to notify the success or failure of your workflows. Currently, Deploy only supports Telegram as notification platform. To enable it you have to select Telegram in the Package settings' `Notify platform` dropdown field. More info in the [docs](https://github.com/mnicolas94/facticus-deploy/wiki/Package-settings)
- Telegram
    - TELEGRAM_TOKEN: same as Telegram deploy platform [above](https://github.com/mnicolas94/facticus-deploy#deploy-platforms-secrets).
    - TELEGRAM_CHAT_ID: same as Telegram deploy platform [above](https://github.com/mnicolas94/facticus-deploy#deploy-platforms-secrets).

### Secrets security
Since secrets generally hold sensitive credentials and information, it is important to know how these variables are used by third-party actions and workflows, like the ones Deploy uses. This [article](https://blog.gitguardian.com/github-actions-security-cheat-sheet/) describes good practices and security measures to take when using Github Actions. I recommend reading it before using this package.

## How to use
After setting up the initial configurations you can start configuring your builds. Just go to the Contexts tab (called Sets in the following video as it is from a previous version) in the Deploy's editor window (`Tools/Facticus/Deploy/Open Deploy editor window`) and create a new Deploy context.

https://github.com/mnicolas94/facticus-deploy/assets/35781652/c9c1969b-4526-4e9a-84fc-48773790ab5d

Each context has the following fields:
1. Repository branch or tag: as the name says, here you must specify the repository's branch or tag you want to build and deploy. The branch or tag must contain the workflow file created in the steps [above](https://github.com/mnicolas94/facticus-deploy#create-the-workflow). Also, you can specify the remote you want to use, in case you have more than one, with the syntax `[remote]/[branch or tag]`. In case you just set `[branch or tag]` remote defaults to `origin`.
2. Override variables: a list of scriptable objects overrides. An override is just a new value that will be overriden in the build for a scriptable object in your project. More info in the [docs](https://github.com/mnicolas94/facticus-deploy/wiki/Override-variables-feature).
3. Platforms: a list of Build-Deploy platforms pairs. Each element of the list describes a platform to build for, a platform to deploy to and whether the build should be flagged with [Development build](https://docs.unity3d.com/Manual/BuildSettings.html).

After configuring your Context press the `Build and Deploy` button to start building your game remotely. The first time, you have to provide a github authentication token. Info on how te get one [here](https://docs.github.com/en/authentication/keeping-your-account-and-data-secure/managing-your-personal-access-tokens) and [here](https://docs.github.com/en/rest/overview/authenticating-to-the-rest-api?apiVersion=2022-11-28). This token will be stored in the project's [Application.persistentDataPath](https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html) so it wont be versioned by git for security reasons.

## Documentation
The full documentation is in the [wiki](https://github.com/mnicolas94/facticus-deploy/wiki). You will find there a more detailed description of each Deploy's feature.

## Known issues
> TODO

## Acknowledgements
- [GameCI](https://game.ci/) for creating and making accessible to everyone the tools used by Deploy to build Unity projects.

## Support me
[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/Q5Q7G6N97)

## License
MIT License
