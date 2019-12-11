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
