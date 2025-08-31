pipeline {
    agent any
    
    environment {
        REGISTRY = "<your-dockerhub-username>",
        IMAGE = "mywebapp",
        TAG = "latest"
    }

    stages {
        stage('Checkout') {
            steps {
                git '<repo-url-here>'
            }
        }

        stage('Build') {
            steps {
                sh "docker build -t $REGISTRY/$IMAGE:$TAG ."
            }
        }

        stage('Push') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'dockerhub-credentials', 
                usernameVariable: 'DOCKERHUB_USERNAME', 
                passwordVariable: 'DOCKERHUB_PASSWORD')]) {
                    sh "echo $DOCKERHUB_PASSWORD | docker login -u $DOCKERHUB_USERNAME --password-stdin"
                    sh "docker push $REGISTRY/$IMAGE:$TAG"
                }
            }
        }
    }
}