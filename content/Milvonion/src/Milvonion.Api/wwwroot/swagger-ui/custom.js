(function () {

    // Enter login endpoint and payload here
    const loginEndpoint = "/xrouteprefixx/v1/account/login";

    const loginPayload = {
        username: "rootuser",
        password: "string",
        deviceId: "string"
    };

    window.addEventListener("load", function () {

        setTimeout(function () {

            const ui = SwaggerUIBundle({
                url: "/xrouteprefixx/docs/v1.0/docs.json",
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [
                    SwaggerUIBundle.presets.apis,
                    SwaggerUIStandalonePreset
                ],
                plugins: [
                    SwaggerUIBundle.plugins.DownloadUrl
                ],
                layout: "StandaloneLayout"
            });

            let reloaded = false;
            let firstRun = true;

            document.addEventListener('click', function (event) {

                if (event.target && (event.target.classList.contains('btn modal-btn') || event.target.classList.contains('auth'))) {
                    if (!firstRun)
                        reloaded = !reloaded;
                    else firstRun = false;

                    if (!reloaded && event.target.ariaLabel === 'Apply credentials') {

                        event.preventDefault();

                        let tokenValue = document.getElementById("api_key_value");

                        console.log(tokenValue.value);

                        if (tokenValue.value)
                        {
                            const token = 'Bearer ' + tokenValue.value; // Dönen token'ı buradan alın

                            // Token'ı Swagger UI'ya ekleme
                            ui.preauthorizeApiKey("Bearer", token);

                            console.log('Auth with token : ', tokenValue.value)

                            return;
                        }

                        // Login isteği gönderme ve token'ı alma
                        fetch(loginEndpoint, {
                            method: "POST",
                            headers: {
                                "Content-Type": "application/json"
                            },
                            body: JSON.stringify(loginPayload)
                        })
                            .then(response => {
                                if (!response.ok) {
                                    throw new Error(`HTTP error! status: ${response.status}`);
                                }
                                return response.json();
                            })
                            .then(data => {
                                const token = 'Bearer ' + data.data.token.accessToken; // Dönen token'ı buradan alın

                                // Token'ı Swagger UI'ya ekleme
                                ui.preauthorizeApiKey("Bearer", token);

                                console.log('Swagger auth successfull! Logged username : ', loginPayload.username)
                            })
                            .catch(error => console.error("Error during login:", error));
                    }
                }
            });

        });
    });
})();