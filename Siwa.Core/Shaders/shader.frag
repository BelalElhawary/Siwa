#version 330 core
out vec4 FragColor;

in vec3 Normal;
in vec3 currentPos;
in vec2 texCoord;

//uniform vec4 lightColor;
uniform sampler2D tex0;
uniform sampler2D tex1;
uniform vec4 lightColor;
uniform vec3 lightPos;
uniform vec3 camPos;

void main() {
    float ambient = 0.20f;
    
    vec3 normal = normalize(Normal);
    vec3 lightDirection = normalize(lightPos - currentPos);
    float diffuse = max(dot(normal, lightDirection), 0.0f);
    
    float specularLight = 0.50f;
    vec3 viewDirection = normalize(camPos - currentPos);
    vec3 refectionDirection = reflect(-lightDirection, normal);
    float specAmount = pow(max(dot(viewDirection, refectionDirection), 0.0f), 8);
    float specular = specAmount * specularLight;

//    FragColor = texture(tex0, texCoord) * lightColor * (diffuse + ambient) + texture(tex1, texCoord).r * specular;
    FragColor = texture(tex0, texCoord) * lightColor * (diffuse + ambient + specular);
}