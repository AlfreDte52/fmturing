// Funciones mínimas de interfaz: atajos de teclado y seguimiento del estado activo.
// No contienen lógica de la máquina: todo el cálculo lo hace FMTuring.Core.
window.fmturing = {
    _keyHandler: null,

    registerKeys: function (dotnet) {
        window.fmturing.unregisterKeys();
        const handler = function (e) {
            const tag = (e.target && e.target.tagName) || "";
            if (["INPUT", "SELECT", "TEXTAREA"].includes(tag)) {
                return;
            }
            let key = null;
            if (e.code === "Space") key = "space";
            else if (e.key === "ArrowRight") key = "right";
            else if (e.key === "ArrowLeft") key = "left";
            if (key) {
                e.preventDefault();
                dotnet.invokeMethodAsync("OnKey", key);
            }
        };
        document.addEventListener("keydown", handler);
        window.fmturing._keyHandler = handler;
    },

    unregisterKeys: function () {
        if (window.fmturing._keyHandler) {
            document.removeEventListener("keydown", window.fmturing._keyHandler);
            window.fmturing._keyHandler = null;
        }
    },

    centerOn: function (element, x, y) {
        if (!element) return;
        element.scrollTo({
            left: Math.max(0, x - element.clientWidth / 2),
            top: Math.max(0, y - element.clientHeight / 2),
            behavior: "smooth"
        });
    }
};
