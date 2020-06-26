# install ssh-server
 - sudo apt update
 - sudo apt upgrade
 - sudo apt install openssh-server
 - sudo systemctl enable ssh
 - sudo systemctl start ssh
 - sudo ufw allow ssh
 - sudo ufw enable
 - sudo ufw status
# commands
- Permission: sudo -i
## Firewall
- sudo ufw allow [port]
- sudo ufw deny [port]
- sudo ufw disable
- sudo ufw enable
- sudo ufw reset
- Liệt kê các cổng: lsoft -i -P -n
- xóa cổng: kill [PID]
# Share folder between windows host and ubuntu guest machine
1. Select ubuntu VM on Virtualbox
3. Click **Setting** button
4. Select **Share Folders** / **Choose folder on windows host** / Okay
5. Start ubuntu virtualbox
6. Install packages
- sudo apt-get update
- sudo apt-get install virtualbox-guest-dkms
- sudo apt-get install virtualbox-guest-utils
- reboot
7. sudo mount -t vboxsf [name folder share] [path on ubuntu machine]
8. set auto amount: echo "sudo mount -t vboxsf [name folder share] [path on ubuntu machine]" >> /home/myusername/.profile
- References: [Share folder between windows host and ubuntu guest machine](https://stackoverflow.com/questions/54336626/how-to-create-virtualbox-shared-folder-between-windows-host-and-ubuntu18-04-gues)

