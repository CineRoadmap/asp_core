// Archivo: CRM.AplicacionWeb\wwwroot\js\legacy\registro.js
// Script heredado con interacciones de la pantalla de registro.

document.addEventListener("DOMContentLoaded", function() {
    // Formulario de registro al que se aplican las validaciones legacy.
    const form = document.querySelector('form');

    form.addEventListener('submit', function(event) {
        event.preventDefault(); // Evitar que se envíe el formulario por defecto

        // Expresiones regulares para validar los campos
        // Patron para validar el nombre de usuario permitido.
        const usernameRegex = /^[a-zA-Z0-9._-]+$/;
        // Patron para validar el alias visible del usuario.
        const nicknameRegex = /^[a-zA-Z0-9._-]+$/;
        // Patron para validar el formato basico del correo electronico.
        const emailRegex = /^[^\s@ñ]+@[^\s@ñ]+\.[^\s@ñ]+$/;
        // Patron para validar telefonos numericos de longitud razonable.
        const phoneRegex = /^\d{6,12}$/;
        // Patron para exigir contrasenas con mayuscula, minuscula y numero.
        const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[A-Za-z\d!"·$%&/()=?'¿¡*+^`Çªº]+$/;

        // Obtener los valores de los campos
        // Campo del nombre de usuario del formulario.
        const usernameField = form.querySelector('input[placeholder="Usuario"]');
        // Campo del alias publico del formulario.
        const nicknameField = form.querySelector('input[placeholder="Nick"]');
        // Campo de correo electronico del formulario.
        const emailField = form.querySelector('input[placeholder="Correo electrónico"]');
        // Campo de telefono del formulario.
        const phoneField = form.querySelector('input[placeholder="Teléfono"]');
        // Campo de contrasena principal del formulario.
        const passwordField = form.querySelector('input[placeholder="Contraseña"]');
        // Campo de confirmacion de contrasena del formulario.
        const confirmPasswordField = form.querySelector('input[placeholder="Repita la contraseña"]');

        // Valor normalizado del nombre de usuario.
        const username = usernameField.value.trim();
        // Valor normalizado del alias.
        const nickname = nicknameField.value.trim();
        // Valor normalizado del correo electronico.
        const email = emailField.value.trim();
        // Valor normalizado del telefono.
        const phone = phoneField.value.trim();
        // Valor normalizado de la contrasena.
        const password = passwordField.value.trim();
        // Valor normalizado de la confirmacion de contrasena.
        const confirmPassword = confirmPasswordField.value.trim();

        // Validar los campos y aplicar estilos si es necesario
        // Estado acumulado de validacion del formulario.
        let valid = true;
        if (!usernameRegex.test(username)) {
            valid = false;
            usernameField.style.borderColor = 'red';
        } else {
            usernameField.style.borderColor = '';
        }
        if (!nicknameRegex.test(nickname) || nickname === username) {
            valid = false;
            nicknameField.style.borderColor = 'red';
        } else {
            nicknameField.style.borderColor = '';
        }
        if (!emailRegex.test(email)) {
            valid = false;
            emailField.style.borderColor = 'red';
        } else {
            emailField.style.borderColor = '';
        }
        if (!phoneRegex.test(phone)) {
            valid = false;
            phoneField.style.borderColor = 'red';
        } else {
            phoneField.style.borderColor = '';
        }
        if (!passwordRegex.test(password)) {
            valid = false;
            passwordField.style.borderColor = 'red';
        } else {
            passwordField.style.borderColor = '';
        }
        if (password !== confirmPassword || confirmPassword === '' || !passwordRegex.test(confirmPassword)) {
            valid = false;
            confirmPasswordField.style.borderColor = 'red';
        } else {
            confirmPasswordField.style.borderColor = '';
        }

        // Mostrar alerta si hay campos inválidos
        if (!valid) {
            // Mensaje acumulado con todos los errores detectados.
            let errorMessage = 'Por favor, corrige los siguientes campos incorrectos:\n\n';
            if (!usernameRegex.test(username)) {
                errorMessage += '- Usuario: solo letras y números\n';
            }
            if (!nicknameRegex.test(nickname) || nickname === username) {
                errorMessage += '- Nick: igual que el usuario, pero  permite los caracteres ".", "_" y "-" \n';
            }
            if (!emailRegex.test(email)) {
                errorMessage += '- Correo electrónico: formato válido (xxx@xxx.xxx).\n';
            }
            if (!phoneRegex.test(phone)) {
                errorMessage += '- Teléfono: entre 6 y 12 dígitos.\n';
            }
            if (!passwordRegex.test(password)) {
                errorMessage += '- Contraseña: al menos una mayúscula, una minúscula y un número.\n';
            }
            if (password !== confirmPassword || confirmPassword === '' || !passwordRegex.test(confirmPassword)) {
                errorMessage += '- Repita la contraseña: debe coincidir exactamente con la contraseña y cumplir con los mismos requisitos.\n';
            }
            alert(errorMessage);
        } else {
            alert('¡Formulario enviado correctamente!');
            // Aquí podrías enviar el formulario si todos los campos son válidos
            // form.submit();
        }
    });
});



