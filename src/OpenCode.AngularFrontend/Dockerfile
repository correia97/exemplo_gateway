FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM nginx:alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY docker-entrypoint.sh /docker-entrypoint.sh

COPY --from=build /app/dist/OpenCode.AngularFrontend/browser /usr/share/nginx/html

RUN chown -R nginx:nginx /usr/share/nginx/html /docker-entrypoint.sh \
 && chmod +x /docker-entrypoint.sh

USER nginx

EXPOSE 80

ENTRYPOINT ["/docker-entrypoint.sh"]
