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
uniform float a;
uniform float b;


vec4 pointLight()
{
    vec3 lightVec = lightPos - currentPos;
    float dist = length(lightVec);
    float inten = 1.0f / (a * dist * dist + b * dist + 1.0f);
    float ambient = 0.20f;

    vec3 normal = normalize(Normal);
    vec3 lightDirection = normalize(lightVec);
    float diffuse = max(dot(normal, lightDirection), 0.0f);

    float specularLight = 0.50f;
    vec3 viewDirection = normalize(camPos - currentPos);
    vec3 refectionDirection = reflect(-lightDirection, normal);
    float specAmount = pow(max(dot(viewDirection, refectionDirection), 0.0f), 16);
    float specular = specAmount * specularLight;
    
    return (texture(tex0, texCoord) * (diffuse * inten + ambient) + texture(tex1, texCoord).r * specular * inten) * lightColor;
}

vec4 directLight()
{
    // ambient lighting
    float ambient = 1.00f;

    // diffuse lighting
    vec3 normal = normalize(Normal);
    vec3 lightDirection = normalize(vec3(1.0f, 1.0f, 0.0f));
    float diffuse = max(dot(normal, lightDirection), 0.0f);

    // specular lighting
    float specularLight = 0.50f;
    vec3 viewDirection = normalize(camPos - currentPos);
    vec3 refectionDirection = reflect(-lightDirection, normal);
    float specAmount = pow(max(dot(viewDirection, refectionDirection), 0.0f), 16);
    float specular = specAmount * specularLight;

    return (texture(tex0, texCoord) * (diffuse * ambient) + texture(tex1, texCoord).r * specular) * lightColor;
}

vec4 spotLight()
{
    float outerCone = 0.90f;
    float innerCone = 0.95f;
    
    // ambient lighting
    float ambient = 1.00f;

    // diffuse lighting
    vec3 normal = normalize(Normal);
    vec3 lightDirection = normalize(lightPos - currentPos);
    float diffuse = max(dot(normal, lightDirection), 0.0f);

    // specular lighting
    float specularLight = 0.50f;
    vec3 viewDirection = normalize(camPos - currentPos);
    vec3 refectionDirection = reflect(-lightDirection, normal);
    float specAmount = pow(max(dot(viewDirection, refectionDirection), 0.0f), 16);
    float specular = specAmount * specularLight;
    
    float angle = dot(vec3(0.0f, -1.0f, 0.0f), -lightDirection);
    float inten = clamp((angle - outerCone) / (innerCone - outerCone), 0.0f, 1.0f);

    return (texture(tex0, texCoord) * (diffuse * inten * ambient) + texture(tex1, texCoord).r * specular * inten) * lightColor;
}

void main() {
    // output final color
    FragColor = pointLight();
}