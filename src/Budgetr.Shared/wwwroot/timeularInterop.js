// Timeular device JavaScript interop for Budgetr
window.timeularInterop = {
    _device: null,
    STORAGE_KEY: "budgetr_timeular_state",

    requestAndConnect: async function () {
        if (!navigator.bluetooth) {
            return {
                success: false,
                message: "Web Bluetooth is not supported in this browser."
            };
        }

        try {
            const device = await navigator.bluetooth.requestDevice({
                filters: [
                    { namePrefix: "Timeular" },
                    { namePrefix: "ZEI" }
                ],
                optionalServices: ["battery_service", "device_information"]
            });

            this._device = device;

            if (device.gatt && !device.gatt.connected) {
                await device.gatt.connect();
            }

            const deviceName = device.name || "Timeular Device";
            const deviceId = device.id || null;

            this.saveState(deviceName, deviceId);

            return {
                success: true,
                deviceName: deviceName,
                deviceId: deviceId
            };
        } catch (error) {
            if (error && error.name === "NotFoundError") {
                return {
                    success: false,
                    message: "Device selection was cancelled."
                };
            }

            return {
                success: false,
                message: error?.message || "Unable to connect to Timeular device."
            };
        }
    },

    disconnect: function () {
        if (this._device && this._device.gatt && this._device.gatt.connected) {
            this._device.gatt.disconnect();
        }
    },

    saveState: function (deviceName, deviceId) {
        const payload = {
            deviceName: deviceName || null,
            deviceId: deviceId || null,
            connectedAtUtc: new Date().toISOString()
        };

        localStorage.setItem(this.STORAGE_KEY, JSON.stringify(payload));
    },

    getSavedState: function () {
        const raw = localStorage.getItem(this.STORAGE_KEY);
        if (!raw) {
            return null;
        }

        try {
            return JSON.parse(raw);
        } catch {
            return null;
        }
    }
};
