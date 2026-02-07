# VPN Support Guide

## Overview
The Tailscale Client now includes built-in support for connecting to external VPN services. This feature allows you to use free VPN providers like VPNBook or configure your own VPN servers while maintaining your Tailscale network connections.

## Supported VPN Protocols

### PPTP (Point-to-Point Tunneling Protocol)
⚠️ **WARNING**: PPTP is deprecated and has known security vulnerabilities. We strongly recommend using L2TP, OpenVPN, or IKEv2 instead.
- **Status**: Deprecated
- **Security**: Low
- **Setup**: Automatic (Windows built-in)

### L2TP (Layer 2 Tunneling Protocol)
- **Status**: Supported
- **Security**: Medium-High
- **Setup**: Automatic (Windows built-in)

### OpenVPN
- **Status**: Supported
- **Security**: High
- **Setup**: Requires OpenVPN client installation
- **Note**: For OpenVPN connections, you'll need to install the OpenVPN client separately and configure it using VPNBook's configuration files.

### IKEv2 (Internet Key Exchange version 2)
- **Status**: Supported
- **Security**: High
- **Setup**: Automatic (Windows built-in)

## Using VPNBook Servers

VPNBook provides free VPN servers with regularly updated credentials. To use VPNBook:

1. **Get Current Credentials**:
   - Visit [https://www.vpnbook.com/](https://www.vpnbook.com/)
   - Find the current password (updated regularly)
   - Note the available server addresses

2. **Configure in Tailscale Client**:
   - Open the Tailscale Client
   - Navigate to the "VPN" tab
   - Pre-configured VPNBook servers are already listed (US1, CA1, DE1)
   - Click "Edit" on a server
   - Enter the current password from vpnbook.com
   - Click "Save"

3. **Connect**:
   - Click "Connect" next to the VPN server you want to use
   - If using PPTP, you'll receive a security warning - we recommend choosing a different protocol
   - The app will attempt to connect using Windows built-in VPN
   - Administrator privileges may be required

## Adding Custom VPN Servers

You can add your own VPN servers:

1. Click "Add VPN Configuration"
2. Fill in the details:
   - **Name**: A friendly name for the server (e.g., "My Company VPN")
   - **Server Address**: The VPN server hostname or IP address
   - **Username**: Your VPN username
   - **Password**: Your VPN password (optional, can enter when connecting)
   - **Protocol**: Choose PPTP, L2TP, OpenVPN, or IKEv2
3. Click "Save"

## Connection Management

### Connecting
- Click the "Connect" button next to any configured VPN server
- If no password is stored, you'll be prompted to enter it
- The connection status will update once connected

### Disconnecting
- When connected, use the "Disconnect" button at the top of the VPN page
- This will terminate the active VPN connection

### Viewing Status
- The VPN status card at the top shows your current connection state
- Shows which VPN server you're connected to (if any)

## Security Considerations

### Password Storage
- Passwords are stored locally on your device in: `%AppData%\TailscaleClient\vpn_configs.json`
- This file is only accessible to your user account
- Consider the security implications of storing VPN passwords locally

### Administrator Privileges
- Creating new VPN connections requires administrator privileges
- Windows will prompt for elevation when needed
- The app uses Windows PowerShell and rasdial for VPN management

### Protocol Selection
- **Avoid PPTP**: Known security vulnerabilities, deprecated protocol
- **Recommended**: L2TP or IKEv2 for Windows built-in support
- **Most Secure**: OpenVPN (requires separate client)

## Troubleshooting

### Connection Fails
1. Verify your credentials are correct
2. Check that you have administrator privileges
3. Ensure the server address is reachable
4. For OpenVPN, verify the OpenVPN client is installed

### Can't Create VPN Connection
- Run the Tailscale Client as Administrator
- Check Windows VPN settings to ensure VPN is enabled
- Verify no conflicting VPN connections exist

### Password Keeps Being Requested
- The password field in the configuration is optional
- If you don't save the password, you'll be prompted each time
- To avoid prompts, enter the password in the Edit dialog and save

## Limitations

### OpenVPN
- OpenVPN requires the OpenVPN client to be installed separately
- Use VPNBook's OpenVPN configuration files with the OpenVPN client
- This app provides information but doesn't directly connect OpenVPN

### Windows Only
- VPN features use Windows built-in VPN capabilities
- Requires Windows 10 or later
- Some features may require specific Windows updates

### Concurrent Connections
- Only one VPN connection can be active at a time through this app
- Connecting to a new VPN will disconnect the current one
- Tailscale connections remain active alongside VPN

## Getting Help

If you encounter issues:
1. Check the troubleshooting section above
2. Visit [vpnbook.com](https://www.vpnbook.com/) for VPNBook-specific help
3. Open an issue on the [GitHub repository](https://github.com/NimuthuGanegoda/tailscale-client)
4. Provide details about your Windows version, VPN protocol, and error messages
