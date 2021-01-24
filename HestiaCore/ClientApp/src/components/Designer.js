import React, { Component } from 'react';

export class Designer extends Component {
    static displayName = Designer.name;

  constructor(props) {
    super(props);
    this.state = {  };
    this.save = this.save.bind(this);
   }

    componentDidMount() {
        this.wfdesigner = new window.WorkflowDesigner({
            name: 'simpledesigner',
            apiurl: 'api/Designer',
            renderTo: 'wfdesigner',
            templatefolder: '/templates/',
            imagefolder: '/Images/',
            graphwidth: 1200,
            graphheight: 600
        });

        let p = { schemecode: "SimpleWF", processid: undefined, readonly: false };

        if (this.wfdesigner.exists(p))
            this.wfdesigner.load(p);
        else
            this.wfdesigner.create();
    }

    shouldComponentUpdate(nextProps) {
        debugger;
    }

  save() {
      this.wfdesigner.schemecode = "SimpleWF";
      var err = this.wfdesigner.validate();
      if (err != undefined && err.length > 0) {
          alert(err);
      }
      else {
          this.wfdesigner.save(function () { alert('The scheme was saved!'); });
      }
  }

  render() {
    return (
      <div>
                    <button className="btn btn-primary" onClick={this.save}>Save</button>

            <div id="wfdesigner"></div>
      </div>
    );
  }
}
