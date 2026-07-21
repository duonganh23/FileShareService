# --- STAGE 1: Build the Vue App ---
FROM node:22 AS build-stage
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json ./
RUN npm install

# Copy the Vue source code and build for production
COPY . .
RUN npm run build

# --- STAGE 2: Serve with Nginx ---
FROM nginx:stable-alpine AS production-stage

# Copy the compiled files from Stage 1 into Nginx's public HTML folder
COPY --from=build-stage /app/dist /usr/share/nginx/html

# Copy our custom Nginx config (we will make this next)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port 80 for web traffic
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
