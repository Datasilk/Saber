(() => {
    var span = document.getElementsByClassName('greetings')[0];
    var hello = ["Hello", "Aloha", "Bonjour", "Hola", "Guten tag", "Ciao", "Olà", "Namaste", "Salaam", "Zdras-tvuy-te", "Konnichiwa", "Merhaba", "Salemetsiz be?", "Szia", "Marhaba", "Jambo", "Ni Hau", "Halo"];
    span.innerHTML = hello[Math.floor(hello.length * Math.random())];
})();