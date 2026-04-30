# Tailscale Client for Windows

[![Build and Release](https://github.com/NimuthuGanegoda/tailscale-client/actions/workflows/build-and-release.yml/badge.svg)](https://github.com/NimuthuGanegoda/tailscale-client/actions/workflows/build-and-release.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Release](https://img.shields.io/github/v/release/NimuthuGanegoda/tailscale-client)](https://github.com/NimuthuGanegoda/tailscale-client/releases/latest)

A modern, high-performance WinUI3-based Tailscale client for Windows. This project provides a clean, native alternative to the official client, focusing on a streamlined user experience and expanded feature set.

## Overview

Tailscale Client is designed to integrate seamlessly with the Windows desktop environment. It serves as a sophisticated UI for the Tailscale daemon, offering advanced management capabilities and a modern aesthetic that aligns with contemporary Windows design principles.

> [!NOTE]
> This application is a client interface and requires the [Tailscale daemon](https://tailscale.com/download/windows) to be installed on the system.

## Key Features

*   **Multi-Account Management:** Seamlessly switch between multiple Tailscale accounts.
*   **Comprehensive Login Flow:** Supports custom control servers, including [Headscale](https://github.com/juanfont/headscale).
*   **QR Code Authentication:** Secure and convenient login via QR codes.
*   **Network Map:** Visualize and manage all devices within your Tailscale network.
*   **Device Details:** Access detailed information (IP, domain, etc.) for every node.
*   **Exit Node Control:** Effortlessly manage routing through exit nodes.
*   **Advanced Settings:** Configure unattended execution, DNS preferences, and subnet acceptance.
*   **VPN Integration:** Support for external VPN protocols (PPTP, L2TP, OpenVPN, IKEv2).

## Preview

<p align="center">
  <img src="https://raw.githubusercontent.com/NimuthuGanegoda/tailscale-client/main/Assets/Images/login.png" width="45%" alt="Login View" style="margin-right: 10px;" />
  <img src="https://raw.githubusercontent.com/NimuthuGanegoda/tailscale-client/main/Assets/Images/preview.png" width="45%" alt="Main View" />
</p>

## Installation

Download the latest version from the [Releases](https://github.com/NimuthuGanegoda/tailscale-client/releases/latest) page. We provide several installation options:

*   **Standard Installer (.exe):** Recommended for most users.
*   **MSI Package (.msi):** Preferred for enterprise deployment and managed environments.
*   **Portable Version:** No installation required, ideal for testing or restricted environments.

## Documentation

For detailed guides and advanced configuration, please refer to:
*   [VPN Support Guide](docs/VPN_GUIDE.md)

## Development

### Prerequisites

*   Visual Studio 2022
*   Windows 10 21H1 or later
*   Windows SDK 22000.194 or later
*   Windows App SDK 1.6
*   .NET 8.0 SDK

### Building from Source

1.  Clone the repository:
    ```bash
    git clone https://github.com/NimuthuGanegoda/tailscale-client.git
    ```
2.  Open `TailscaleClient.csproj` in Visual Studio 2022.
3.  Restore NuGet packages and build the solution.

## Contributing

We welcome contributions from the community. Please review our issue tracker for planned features or bug reports. For major changes, please open an issue first to discuss your proposal.

## License

This project is licensed under the MIT License. See the [LICENSE.txt](LICENSE.txt) file for details.

## Acknowledgements

*   [Tailscale](https://tailscale.com) for their exceptional open-source contributions.
*   [QRCoder](https://github.com/codebude/QRCoder) for QR code generation.
*   [Velopack](https://velopack.io) for modern desktop deployment.
