# Download and install FNM on windows
- Download version FNM for windows from: https://github.com/Schniz/fnm
- Extract the downloaded archive to: %userprofile%\AppData\Local\fnm
- Add the following to your system or user environment variables:
  - Variable name: PATH
  - Variable value: %userprofile%\AppData\Local\fnm
- Verify the installation by running the command: fnm --version
- Create a file named and content as below: %userprofile%\fnm_init.cmd
    + https://github.com/Schniz/fnm?tab=readme-ov-file#windows-command-prompt-aka-batch-aka-wincmd
    + 
`
@echo off
:: for /F will launch a new instance of cmd so we create a guard to prevent an infnite loop
if not defined FNM_AUTORUN_GUARD (
    set "FNM_AUTORUN_GUARD=AutorunGuard"
    FOR /f "tokens=*" %%z IN ('fnm env --use-on-cd') DO CALL %%z
)
`

- Run command as bleow to registry editor:

`
reg add "HKEY_CURRENT_USER\SOFTWARE\Microsoft\Command Processor" /v AutoRun /t REG_SZ /d "%userprofile%\fnm_init.cmd" /f
`
- Restart your terminal and verify that FNM is working by running the command: fnm --version
- Command to use FNM to install and use node version:
    + Install node version: fnm install <version>
    + Use node version: fnm use <version>
    + Set default node version: fnm default <version>
    + List installed node versions: fnm list
    + List available node versions: fnm list-remote
    + Uninstall node version: fnm uninstall <version>
