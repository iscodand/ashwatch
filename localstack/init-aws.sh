#!/bin/bash
echo "Creating SNS topic and SQS queue..."

awslocal sns create-topic \
  --name ashwatch-logs.fifo \
  --attributes FifoTopic=true,ContentBasedDeduplication=false

awslocal sqs create-queue \
  --queue-name ashwatch-logs.fifo \
  --attributes FifoQueue=true,ContentBasedDeduplication=false

awslocal sns subscribe \
  --topic-arn arn:aws:sns:us-east-1:000000000000:ashwatch-logs.fifo \
  --protocol sqs \
  --notification-endpoint arn:aws:sqs:us-east-1:000000000000:ashwatch-logs.fifo \
  --attributes RawMessageDelivery=true

echo "LocalStack init complete."
