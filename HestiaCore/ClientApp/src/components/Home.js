import React, { Component } from 'react';
import { Auth } from 'aws-amplify';

export class Home extends Component {
    static displayName = Home.name;

    componentDidMount() {
      this.populateTickets();
    }

    async populateTickets() {
        await Auth.currentSession().then(async res => {
            let jwt = res.getAccessToken().getJwtToken()

            const response = await fetch("api/tickets", {
                headers: {
                    'Authorization': 'Bearer ' + jwt
                }
            });
            const data = await response.json();
            this.setState({ tickets: data, loading: false });
        })

        
    }

  render () {
    return (
      <div>
            <div>
                {this.state?.tickets.map(ticket => <a href={ `/ticket/${ticket.id}` }>{ticket.assignee}&nbsp;{ ticket.nextState }</a>)}
            </div>
      </div>
    );
  }
}
