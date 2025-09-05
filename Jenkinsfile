pipeline {
    agent any

    environment {
        REGISTRY = 'docker.io/gourav8'
        IMAGE = 'mywebapp'
        TAG = "${env.BRANCH_NAME}-${env.BUILD_NUMBER}"
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'main', url: 'https://github.com/CodeAtom1/mywebapp'
            }
        }

        stage('Build Image') {
            steps {
                sh """
                    docker build -t $REGISTRY/$IMAGE:$TAG .
                """
            }
        }

        stage('Run & Health Check') {
            steps {
                withCredentials([
                    string(credentialsId: 'MySqlConnectionString', variable: 'DB_CONN'),
                    string(credentialsId: 'RedisConnectionString', variable: 'REDIS_CONN')
                ]) {
                    sh '''
                        # Free port 8090 if in use by a container
                        EXISTING_CID=$(docker ps -q --filter "publish=8090")
                        if [ -n "$EXISTING_CID" ]; then
                            echo "Stopping container on port 8090 (CID: $EXISTING_CID)"
                            docker stop "$EXISTING_CID" && docker rm "$EXISTING_CID"
                        fi

                        # Run container with secrets from Jenkins credentials
                        CID=$(docker run -d \
                            -e ConnectionStrings__DefaultConnection=$DB_CONN \
                            -e Redis__Configuration=$REDIS_CONN \
                            -p 8090:5001 $REGISTRY/$IMAGE:$TAG)

                        echo "Started container: $CID"

                        # Wait for app to boot
                        sleep 15

                        # Health check with retries
                        for i in {1..5}; do
                        if curl -f http://localhost:8090/health; then
                            echo "Health check passed"
                            docker stop $CID && docker rm $CID
                            exit 0
                        fi
                        echo "Health check failed, retrying in 5s..."
                        sleep 5
                        done

                        echo "Health check failed after retries"
                        docker stop $CID && docker rm $CID
                        exit 1
                    '''
                }
            }
        }

        stage('Push Image') {
            steps {
                script {
                    def pushImage = { tag ->
                        withCredentials([usernamePassword(
                            credentialsId: 'dockerhub-credentials',
                            usernameVariable: 'DOCKERHUB_USERNAME',
                            passwordVariable: 'DOCKERHUB_PASSWORD'
                        )]) {
                            sh '''
                                echo $DOCKERHUB_PASSWORD | docker login -u $DOCKERHUB_USERNAME --password-stdin
                                docker tag $REGISTRY/$IMAGE:$TAG $REGISTRY/$IMAGE:${tag}
                                docker push $REGISTRY/$IMAGE:${tag}
                            '''
                        }
                    }

                    pushImage(env.TAG)
                    if (env.BRANCH_NAME == "main") {
                        pushImage("latest")
                    }
                }
            }
        }

        stage('Deploy') {
            steps {
                sh """
                    echo "Deploying $REGISTRY/$IMAGE:$TAG ..."
                    docker run -d -p 8090:5001 $REGISTRY/$IMAGE:$TAG
                    # Example: docker-compose or kubectl
                    # docker-compose -f docker-compose.prod.yml up -d
                    # or: kubectl set image deployment/mywebapp mywebapp=$REGISTRY/$IMAGE:$TAG
                """
            }
        }
    }

    post {
        always {
            sh "docker logout || true"
        }
    }
}