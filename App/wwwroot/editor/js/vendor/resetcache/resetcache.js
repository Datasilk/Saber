(function () {
    $('.reset-cache button').on('click', () => {
        $.ajax({
            url: '/ResetCache',
            success: () => {
                alert('Website cache reset & recompile complete');
            }
        });
    });
})();