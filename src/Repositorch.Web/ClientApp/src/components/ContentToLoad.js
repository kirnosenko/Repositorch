import React, { Component } from 'react';
import Loading from './Loading';

export class ContentToLoad extends Component {

    constructor(props) {
        super(props);
        this.state = { data: null };
    }

    componentDidMount() {
        this.loadData();
    }

    render() {
        let contents = this.state.data === null
            ? <Loading />
            : this.props.renderData(this.state.data);

        return (
            <div>
                {contents}
            </div>
        );
    }

    async loadData() {
        fetch(this.props.url)
            .then((response) => {
                if (!response.ok) throw new Error(response.status);
                else return response.json();
            })
            .then((data) => {
                this.setState({ data: data });
            });
    }
}
