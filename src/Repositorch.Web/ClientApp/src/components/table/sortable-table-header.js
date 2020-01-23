import React, { Component } from 'react';

class SortableTableHeaderItem extends Component {

  static defaultProps = {
    headerProps: {},
    sortable: true
  }

  onClick(e) {
    if (this.props.sortable)
      this.props.onClick(this.props.index);
  }

  render() {
    let sortIcon;
    if (this.props.sortable) {
      if (this.props.iconBoth) {
        sortIcon = this.props.iconBoth;
      } else {
        sortIcon = <span style={{ float: 'right', color: 'transparent' }}>↓</span>
      }
      if (this.props.sorting == "desc") {
        if (this.props.iconDesc) {
          sortIcon = this.props.iconDesc;
        } else {
          sortIcon = <span style={{ float: 'right' }}>↓</span>
        }
      } else if (this.props.sorting == "asc") {
        if (this.props.iconAsc) {
          sortIcon = this.props.iconAsc;
        } else {
          sortIcon = <span style={{ float: 'right' }}>↑</span>
        }
      }
    }

    return (
      <th
        style={this.props.style}
        onClick={this.onClick.bind(this)}
        {...this.props.headerProps} >
        {this.props.header}
        {sortIcon}
      </th>
    );
  }
}

export default class SortableTableHeader extends Component {

  onClick(index) {
    this.props.onStateChange.bind(this)(index);
  }

  render() {
    const headers = this.props.columns.map(((column, index) => {
      const sorting = this.props.sortings[index];
      return (
        <SortableTableHeaderItem
          sortable={column.sortable}
          key={index}
          index={index}
          header={column.header}
          sorting={sorting}
          onClick={this.onClick.bind(this)}
          style={column.headerStyle}
          headerProps={column.headerProps}
          iconDesc={this.props.iconDesc}
          iconAsc={this.props.iconAsc}
          iconBoth={this.props.iconBoth} />
      );
    }).bind(this));

    return (
      <thead>
        <tr>
          {headers}
        </tr>
      </thead>
    );
  }
}
