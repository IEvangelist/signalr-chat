window.app = {
    notify: (title, message) => {
        if (toastr) {
            toastr.info(message, title);
        }
    },
    updateScroll: () => {
        const element = document.querySelector('html');
        element.scrollTop = element.scrollHeight;
    }
};

if (toastr) {
    toastr.options = {
        toastClass: 'info',
        iconClasses: {
            info: 'alert alert-info'
        },
        newestOnTop: true,
        positionClass: 'toast-top-center',
        preventDuplicates: true
    };
}