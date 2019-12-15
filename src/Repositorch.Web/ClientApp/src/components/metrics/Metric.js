import React, { Component } from 'react';
import Loading from '../Loading';

export class Metric extends Component {

    constructor(props) {
        super(props);
        this.state = { data: null };
    }

    componentDidMount() {
        this.populateData();
    }
    
    render() {
        let contents = this.state.data === null
            ? <Loading />
            : this.props.renderData(this.state.data);

        return (
            <div>
                <h2 id="tabelLabel">{this.props.title}</h2>
                {contents}
            </div>
        );
    }

    async populateData() {
        const response = await fetch('api/data/CalculateMetrics/'.concat(this.props.metric));
        const data = await response.json();
        this.setState({ data: data });
    }
}
