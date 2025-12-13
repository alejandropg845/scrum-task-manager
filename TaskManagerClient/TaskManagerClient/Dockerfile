# compilar
FROM node:18-alpine as build
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN npm run build --prod

# servidor Nginx para producción
FROM nginx:alpine

# copia el build de Angular a la carpeta de Nginx
COPY --from=build /app/dist/task-manager/browser /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
