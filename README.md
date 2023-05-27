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

# How to install
### Via OpenUPM
> TODO
### Via Git URL
Install dependencies first
- open Package Manager
- click <kbd>+</kbd>
- select <kbd>Add from Git URL</kbd>
- paste `https://github.com/mackysoft/Unity-SerializeReferenceExtensions.git?path=Assets/MackySoft/MackySoft.SerializeReferenceExtensions`
- click <kbd>Add</kbd>

Then `https://github.com/mnicolas94/UnityUtils.git` with the same method. If you get errors installing this dependency make sure you have Unity's Localization `com.unity.localization` and new Input System `com.unity.inputsystem` packages installed. This is a current limitation of this dependency that will be addressed as soon as possible.![Package Manager_3](https://github.com/mnicolas94/facticus-deploy/assets/35781652/6bd20465-1ed4-4390-b5c0-f2907b49b154)

And finally, install the Deploy package itself `https://github.com/mnicolas94/facticus-deploy.git`.

# Requirements
1. Your project has to be hosted on GitHub.
2. I only tested it on 2021.1+ Unity versions. Versions prior to that should work, but I'm not sure.
3. ... TODO

# How to use
### Create the workflow
1. Inside Unity open the Deploy's editor window with `Tools/Facticus/Deploy/Edit sets`.
![facticus-deploy - SampleScene - Windows, Mac, Linux - Unity 2021 3 15f1 Personal DX11](https://github.com/mnicolas94/facticus-deploy/assets/35781652/01eadaa7-b4b7-40aa-a8f0-d60dbd5914da)
2. [optional] Dock the window somewhere within your editor. This is recommended due to a known issue where the window wont open anymore if you lose its focus while is floating.
3. Click on the `Package settings` tab.
4. 

> TODO

# Limitations
> TODO

# Known issues
> TODO

# FAQ??
> TODO

## License

MIT License
