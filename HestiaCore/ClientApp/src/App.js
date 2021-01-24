import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';
import { FetchData } from './components/FetchData';
import { Designer } from './components/Designer';
import { Ticket } from './components/Ticket';
import Amplify, { Auth } from 'aws-amplify';
import { withAuthenticator, AmplifySignOut } from '@aws-amplify/ui-react';

import './custom.css'

let redirectUrl = window.origin;
if (!redirectUrl) {
    redirectUrl = `${window.location.protocol}//${window.location.hostname}${window.location.port ? `:${window.location.port}` : ''}`;
}

Amplify.configure({
    Auth: {
        region: 'us-east-1',
        userPoolId: 'us-east-1_zOWlmS6q5',
        userPoolWebClientId: '1qrnki8m3p5eagj2jggltphccb',
        oauth: {
            domain: 'sso.sd.kcftech.com',
            scope: ['profile', 'openid'],
            redirectSignIn: `${redirectUrl}/`,
            redirectSignOut: `${redirectUrl}/login`,
            responseType: 'code',
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
                  <Route path='/designer' component={Designer} />
                  <Route path='/fetch-data' component={FetchData} />
                  <Route path='/ticket/:id' component={Ticket} />
              </Layout>
          </div>
    );
  }
}

export default withAuthenticator(App);
