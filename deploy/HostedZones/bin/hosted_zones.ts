#!/usr/bin/env node
import 'source-map-support/register';
import * as cdk from '@aws-cdk/core';
import { HostedZonesStack } from '../lib/hosted_zones-stack';

const app = new cdk.App();
new HostedZonesStack(app, 'HostedZonesStack');
