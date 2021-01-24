import React, { Component } from 'react';

export class Counter extends Component {
  static displayName = Counter.name;

  constructor(props) {
    super(props);
    this.state = { currentCount: 0 };
    this.incrementCounter = this.incrementCounter.bind(this);
    }

    componentDidMount() {
        var wfdesigner = new window.WorkflowDesigner({
            name: 'simpledesigner',
            apiurl: 'api/Designer',
            renderTo: 'wfdesigner',
            templatefolder: '/templates/',
            imagefolder: '/Images/',
            graphwidth: 1200,
            graphheight: 600
        });

        let p = { schemecode: "SimpleWF", processid: undefined, readonly: false };

        if (wfdesigner.exists(p))
            wfdesigner.load(p);
        else
            wfdesigner.create();
    }

    shouldComponentUpdate(nextProps) {
        debugger;
    }

  incrementCounter() {
    this.setState({
      currentCount: this.state.currentCount + 1
    });
  }

  render() {
    return (
      <div>
        <h1>Counter</h1>

        <p>This is a simple example of a React component.</p>

        <p aria-live="polite">Current count: <strong>{this.state.currentCount}</strong></p>

            <button className="btn btn-primary" onClick={this.incrementCounter}>Increment</button>

            <div id="wfdesigner"></div>
      </div>
    );
  }
}
