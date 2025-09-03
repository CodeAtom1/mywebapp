pipeline {
    agent any
    
    options {
        ansiColor('xterm')
    }
    
    environment {
        DB_CONN    = credentials('MySqlConnectionString')
        REDIS_CONN = credentials('RedisConnectionString')
    }

    stages {
        stage('Checkout') {
            steps {
                git 'https://github.com/CodeAtom1/mywebapp'
            }
        }

        stage('Build') {
            steps {
                sh '''
                    docker run -d \
                        -e ConnectionStrings__DefaultConnection="$DB_CONN" \
                        -e Redis__Configuration="$REDIS_CONN" \
                        -p 8090:80 mywebapp:latest
                '''
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