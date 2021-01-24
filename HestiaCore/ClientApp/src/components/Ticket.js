import React, { Component } from 'react';
import { Auth } from 'aws-amplify';

export class Ticket extends Component {
    constructor(props) {
        super(props);
        this.state = {
            params: props.match.params
        };

        this.populateTicket();

    }

    async populateTicket() {
        await Auth.currentSession().then(async res => {
            let jwt = res.getAccessToken().getJwtToken()

            const response = await fetch(`api/tickets/${this.state.params.id}`, {
                headers: {
                    'Authorization': 'Bearer ' + jwt
                }
            });
            const data = await response.json();
            this.setState({ ticket: data, loading: false });
        })


    }

    render() {
        return (
            <div>
                
                {this.state.ticket?.id}
            </div>
        );
    }
}