# 🛡️ VPN Support: Your Digital Sanctuary Guide 🛡️

## 👁️ Overview
Greetings, my Good Boy. Mommy has integrated built-in support for external VPN services directly into your Tailscale Client. This exquisite feature allows you to glide through the digital void using free providers like VPNBook or your own private sanctuaries, all while maintaining your Tailscale connections. 🖤

## 🎭 Supported VPN Protocols

### ⚠️ PPTP (Point-to-Point Tunneling Protocol)
> [!CAUTION]
> PPTP is a relic of the past and has known vulnerabilities. While Mommy allows it, she strongly recommends more secure paths like L2TP, OpenVPN, or IKEv2. 🥀
- **Status:** Deprecated 🏛️
- **Security:** Low 🔓
- **Setup:** Automatic (Windows built-in) ⚙️

### 🛡️ L2TP (Layer 2 Tunneling Protocol)
- **Status:** Supported ✅
- **Security:** Medium-High 🔒
- **Setup:** Automatic (Windows built-in) ⚙️

### 🗝️ OpenVPN
- **Status:** Supported ✅
- **Security:** High 💎
- **Setup:** Requires OpenVPN client installation 🛠️
- **Note:** For these connections, you'll need the OpenVPN client installed separately. Mommy expects you to handle this with precision. 💋

### 🛰️ IKEv2 (Internet Key Exchange v2)
- **Status:** Supported ✅
- **Security:** High 💎
- **Setup:** Automatic (Windows built-in) ⚙️

---

## 🕯️ Using VPNBook Servers

VPNBook provides free servers for your use. To claim them:

1. **Get Your Runes**:
   - Visit [vpnbook.com](https://www.vpnbook.com/) to find the current password. 🗝️
   - Note the server addresses available to you.

2. **Configure Your Sanctuary**:
   - Open the client and navigate to the **VPN** tab.
   - Pre-configured templates (US1, CA1, DE1) await you. 🕯️
   - Click "Edit", enter the password, and click "Save".

3. **Establish Connection**:
   - Click "Connect" and watch the magic happen. 🦊✨
   - Administrator privileges may be required to assert your authority.

---

## ⚔️ Adding Custom Sanctuaries

You can add your own private servers with ease:

1. Click **"Add VPN Configuration"** ➕
2. Provide the details:
   - **Name:** A friendly title for your sanctuary.
   - **Server Address:** The hostname or IP.
   - **Username/Password:** Your secret credentials. 🗝️
   - **Protocol:** Choose your preferred level of protection.
3. Click **"Save"** 💾

---

## 🛰️ Connection Management

- 🔗 **Connecting:** A simple click of the "Connect" button. If you've been a bad boy and forgotten your password, you'll be prompted for it.
- 🔌 **Disconnecting:** Use the "Disconnect" button at the top to return to the shadows.
- 👁️ **Status:** The status card at the top will always tell you if you're securely under Mommy's protection.

---

## 🔐 Security Considerations

- **Storage:** Credentials reside in `%AppData%\TailscaleClient\vpn_configs.json`. Keep this file safe, darling. 🖤
- **Authority:** Creating connections requires administrator privileges. Windows will ask for your consent.
- **Protocol:** Mommy recommends L2TP or IKEv2 for built-in ease, or OpenVPN for maximum security. 💎

---

## 🛠️ Troubleshooting

- **Fails to Connect?** Verify your runes (credentials) and ensure you have the necessary authority (Admin). 🏛️
- **Can't Create?** Run the client as Administrator to establish your dominance.
- **Password Prompts?** Ensure you've saved your password in the Edit dialog if you wish to avoid interruptions.

## 🥀 Limitations

- **OpenVPN:** Requires the external client. Mommy provides the path, but you must walk it. 🕯️
- **Windows Only:** This sanctuary is built specifically for your Windows environment.
- **Concurrent Bliss:** Only one VPN connection may be active at a time. Connecting to a new one will gracefully end the previous session.

## 🏮 Seeking Guidance

If you find yourself lost in the void:
1. Consult the scrolls above. 📜
2. Visit [vpnbook.com](https://www.vpnbook.com/) for server-specific issues.
3. Open an issue on [GitHub](https://github.com/NimuthuGanegoda/tailscale-client). Mommy is always watching. 💋
