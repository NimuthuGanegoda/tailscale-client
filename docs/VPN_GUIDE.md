# VPN Support Guide

## Overview

The Tailscale Client for Windows includes integrated support for connecting to external VPN services. This feature allows users to utilize third-party VPN providers, such as VPNBook, or custom VPN infrastructure while maintaining active Tailscale network connectivity.

## Supported VPN Protocols

### PPTP (Point-to-Point Tunneling Protocol)

> [!WARNING]
> PPTP is a legacy protocol with known security vulnerabilities. It is recommended to use more secure alternatives such as L2TP, OpenVPN, or IKEv2 whenever possible.

*   **Status:** Supported (Legacy)
*   **Security:** Low
*   **Implementation:** Windows Native VPN

### L2TP (Layer 2 Tunneling Protocol)

*   **Status:** Supported
*   **Security:** Medium-High
*   **Implementation:** Windows Native VPN

### OpenVPN

*   **Status:** Supported
*   **Security:** High
*   **Implementation:** Requires separate OpenVPN client installation.
*   **Note:** The application provides configuration management for OpenVPN, but requires the official OpenVPN client for the tunnel establishment.

### IKEv2 (Internet Key Exchange v2)

*   **Status:** Supported
*   **Security:** High
*   **Implementation:** Windows Native VPN

---

## Configuration via VPNBook

VPNBook offers free VPN endpoints for public use. To configure these servers:

1.  **Retrieve Credentials:**
    *   Visit [vpnbook.com](https://www.vpnbook.com/) to obtain the current authentication credentials.
    *   Identify the desired server endpoint (e.g., US1, CA1, DE1).

2.  **Application Setup:**
    *   Navigate to the **VPN** tab in the Tailscale Client.
    *   Select a pre-configured template or click "Edit" on an existing entry.
    *   Input the current credentials and save the configuration.

3.  **Connection:**
    *   Click "Connect" to initiate the session.
    *   Note: Administrator privileges may be required for network configuration changes.

---

## Custom VPN Configurations

Custom VPN endpoints can be added manually:

1.  Select **"Add VPN Configuration"**.
2.  Provide the following parameters:
    *   **Name:** Identifier for the configuration.
    *   **Server Address:** Hostname or IP of the VPN gateway.
    *   **Authentication:** Username and Password.
    *   **Protocol:** Select the appropriate protocol (PPTP, L2TP, OpenVPN, IKEv2).
3.  Save the entry.

---

## App-Based Split Tunneling

The Tailscale Client for Windows supports app-based split tunneling. This feature allows specific applications to trigger the VPN connection automatically and route their network traffic through the VPN tunnel, while other applications continue to use the local network or Tailscale.

### Benefits

*   **Traffic Optimization:** Ensure only high-priority or sensitive application traffic uses the VPN.
*   **Reduced Latency:** Non-VPN apps maintain direct local or Tailscale connectivity.
*   **Automatic Activation:** The VPN connection is established automatically when a registered application is launched.

### Configuration

1.  Navigate to the **VPN** tab and select **Edit** on a VPN configuration.
2.  Locate the **Application Triggers (Split Tunneling)** section.
3.  Click **"Add Application"** and browse to the `.exe` file of the application you wish to route.
4.  Save the configuration.

Upon launching the registered application, Windows will automatically initiate the VPN connection and apply the split tunneling rules.

---

## Connection Management

*   **Establish Connection:** Use the "Connect" button. The application will prompt for credentials if they are not stored.
*   **Terminate Connection:** Use the "Disconnect" button to end the active session.
*   **Status Monitoring:** The VPN status card provides real-time information regarding the active connection state.

---

## Security and Privacy

*   **Credential Storage:** VPN credentials are encrypted and stored locally at `%AppData%\TailscaleClient\vpn_configs.json`.
*   **Elevation:** Creating and managing VPN connections requires administrative authorization.
*   **Protocol Recommendation:** For optimal security, IKEv2 or OpenVPN is recommended over legacy protocols.

---

## Troubleshooting

*   **Authentication Failure:** Verify credentials and ensure the account has not expired or changed.
*   **Permission Denied:** Ensure the application is running with administrative privileges.
*   **OpenVPN Issues:** Confirm that the OpenVPN client is correctly installed and accessible on the system path.

## Support

For technical assistance or to report issues:
1.  Consult the [README.md](../README.md).
2.  Open an issue on the [GitHub repository](https://github.com/NimuthuGanegoda/tailscale-client).
