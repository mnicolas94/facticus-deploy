# Deploy

Deploy is a Unity package that allows you to build your game for multiple platforms and deploy them to various stores, remotely (rather than on your workstation) and with a single click inside Unity. It uses [GitHub Actions](https://github.com/features/actions) under the hood to run the builds on GitHub servers.

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
3. ... TODO

## Requirements
1. Your project has to be hosted on GitHub.
2. I only tested it on 2021.1+ Unity versions. Versions prior to that should work, but I'm not sure.
3. ... TODO

## How to install
### Via OpenUPM
> TODO
### Via Git URL
Install dependencies first
- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/mackysoft/Unity-SerializeReferenceExtensions.git?path=Assets/MackySoft/MackySoft.SerializeReferenceExtensions`
- click <kbd>Add</kbd>

Then `https://github.com/mnicolas94/UnityUtils.git` with the same method. If you get errors installing this dependency make sure you have Unity's Localization `com.unity.localization` and new Input System `com.unity.inputsystem` packages installed. This is a current limitation of this dependency that will be addressed as soon as possible.
![Package Manager_3](https://github.com/mnicolas94/facticus-deploy/assets/35781652/6bd20465-1ed4-4390-b5c0-f2907b49b154)

And finally, install the Deploy package itself `https://github.com/mnicolas94/facticus-deploy.git`.

## How to setup
### Create the workflow
1. Inside Unity open the Deploy's editor window with `Tools/Facticus/Deploy/Edit sets`.
![facticus-deploy - SampleScene - Windows, Mac, Linux - Unity 2021 3 15f1 Personal DX11](https://github.com/mnicolas94/facticus-deploy/assets/35781652/01eadaa7-b4b7-40aa-a8f0-d60dbd5914da)
2. [optional] Dock the window somewhere within your editor. This is recommended due to a known issue where the window wont open anymore if you lose its focus while is floating.
3. Click on the `Package settings` tab.![facticus-deploy-setup-guide - SampleScene - Windows, Mac, Linux - Unity 2021 3 15f1 Personal DX11](https://github.com/mnicolas94/facticus-deploy/assets/35781652/b0b8dd6c-c31b-4fea-b626-ad98a6d8e555)
4. First configure the `Git Directory` field if needed. It should point to your git repository's root directory relative to your Unity project's root. Commonly, both directories match, in which case you should leave the field empty.
5. Then, click the `Generate workflow` button and choose whatever name you want. This will be the name of the GitHub Actions workflow yml file created to build and deploy the project. Just take this in mind to select a name different from other existent workflows you already have in your project.
6. Leave the other fields as they are for now and commit and push your changes. This will push the created workflow to GitHub in order for it to be called later.

### Configuring Secrets
You need to configure some [Secrets](https://docs.github.com/en/actions/security-guides/encrypted-secrets) in your repository in order to use this package. To add secrets you have to go to your repository's `Settings -> Secrets and variables -> Actions`. Almost each build and deploy platform has its own set of secrets that need to be configured in order to use it. You only need to configure the secrets of platforms you intend to use.

#### General secrets (required for any build and deploy platform)
- UNITY_LICENSE: The contents of a Unity license file (Unity_v20XX.x.ulf). Info on how to get it [here](https://game.ci/docs/github/activation).

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
A notification platform is a channel to notify the success or failure of your workflows. Currently Deploy only supports Telegram as notification platform. To enable it you have to select Telegram in the Package settings' `Notify platform` dropdown field.
![UnityEditor IMGUI Controls AdvancedDropdownWindow](https://github.com/mnicolas94/facticus-deploy/assets/35781652/f09eb62e-b5f8-4d46-b718-8b6c7ac2ac55)
    - TELEGRAM_TOKEN: same as Telegram deploy platform [above](https://github.com/mnicolas94/facticus-deploy/edit/main/README.md#deploy-platforms-secrets).
    - TELEGRAM_CHAT_ID: same as Telegram deploy platform [above](https://github.com/mnicolas94/facticus-deploy/edit/main/README.md#deploy-platforms-secrets).

## How to use
> TODO

## Override variables feature
> TODO

## Limitations
> TODO

## Known issues
> TODO

## FAQ??
> TODO

## Acknowledgements
> TODO. Acknowledge GameCI and actions used in this

## Support me
> TODO. add ko-fi

## License

MIT License
