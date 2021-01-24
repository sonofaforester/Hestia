import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Counter } from './components/Counter';
import Amplify, { Auth } from 'aws-amplify';
import { withAuthenticator, AmplifySignOut } from '@aws-amplify/ui-react';

import './custom.css'


Amplify.configure({
    Auth: {

        // REQUIRED only for Federated Authentication - Amazon Cognito Identity Pool ID
        //identityPoolId: 'XX-XXXX-X:XXXXXXXX-XXXX-1234-abcd-1234567890ab',

        // REQUIRED - Amazon Cognito Region
        region: 'us-east-1',
        // OPTIONAL - Amazon Cognito User Pool ID
        userPoolId: 'us-east-1_zOWlmS6q5',

        // OPTIONAL - Amazon Cognito Web Client ID (26-char alphanumeric string)
        userPoolWebClientId: '1qrnki8m3p5eagj2jggltphccb',

        // OPTIONAL - Enforce user authentication prior to accessing AWS resources or not
        //mandatorySignIn: false,

        // OPTIONAL - Configuration for cookie storage
        // Note: if the secure flag is set to true, then the cookie transmission requires a secure protocol
        //cookieStorage: {
        //    // REQUIRED - Cookie domain (only required if cookieStorage is provided)
        //    domain: '.yourdomain.com',
        //    // OPTIONAL - Cookie path
        //    path: '/',
        //    // OPTIONAL - Cookie expiration in days
        //    expires: 365,
        //    // OPTIONAL - See: https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Set-Cookie/SameSite
        //    sameSite: "strict" | "lax",
        //    // OPTIONAL - Cookie secure flag
        //    // Either true or false, indicating if the cookie transmission requires a secure protocol (https).
        //    secure: true
        //},

        // OPTIONAL - customized storage object
        //storage: MyStorage,

        // OPTIONAL - Manually set the authentication flow type. Default is 'USER_SRP_AUTH'
        //authenticationFlowType: 'USER_PASSWORD_AUTH',

        // OPTIONAL - Manually set key value pairs that can be passed to Cognito Lambda Triggers
        //clientMetadata: { myCustomKey: 'myCustomValue' },

        // OPTIONAL - Hosted UI configuration
        oauth: {
            domain: 'sso.sd.kcftech.com',
            scope: ['openid', 'profile'],
            redirectSignIn: 'http://localhost:3000/',
            redirectSignOut: 'http://localhost:3000/',
            responseType: 'code' // or 'token', note that REFRESH token will only be generated when the responseType is code
        }
    }
});

export class App extends Component {
    static displayName = App.name;

    

  render () {
      return (
        <div>
              <AmplifySignOut />
           
              <Layout>
                <Route exact path='/' component={Home} />
                <Route path='/counter' component={Counter} />
                <Route path='/fetch-data' component={FetchData} />
              </Layout>
          </div>
    );
  }
}

export default withAuthenticator(App);
