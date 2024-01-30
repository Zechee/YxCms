﻿# YxCms
# Asp.net Core MVC 微型迷你内容管理系统

## 一、源码特点

本项目是一个基于 Asp.net Core MVC 构建的微型内容管理系统，它具备以下特点：

1. **基本功能**：包含增删查改、导入导出、上传下载、表格打印以及数据报表等核心功能。
2. **适用人群**：适合不同水平的开发人员和管理人员使用，包括但不限于学生、初级/中级/高级程序员、前端工程师、架构师、产品经理、站长等。
3. **应用场景**：可直接用于生产环境实际项目中，或作为学习和研究的优秀案例。

## 二、技术栈

本项目采用以下技术栈：

1. **前端 UI 框架**：Layui + Layui miniAdmin + jQuery，特点是高颜值、高逼格。
2. **富文本框架**：wangEditor，界面清新干净，操作简便。
3. **数据报表框架**：百度 Echarts，功能齐全，专业化。
4. **Excel 框架**：NPOI，方便的 Excel 数据处理能力。
5. **后端 ORM 框架**：Dapper，小型 ORM 性能之王。
6. **后端架构**：三层架构 + Asp.net Core MVC + .NET 6.0，保证了项目的稳定性和快速响应。

### 其它注意事项

在配置 IIS 时，请将应用程序池的高级管理设置为 `LocalSystem`，以确保能够正确读取数据库中的数据。

## 三、菜单功能

### 权限管理

- **用户管理**：实现系统用户的增删改查，分配角色和系统权限。
- **菜单管理**：管理系统的无限层级树形菜单，包括添加、修改、设置图标、控制按钮等。
- **角色管理**：对系统用户进行管理，并分配用户和系统权限。
- **权限管理**：为不同角色分配菜单和按钮权限。
- **网站配置**：设定和修改网站名称、域名、标题描述、版权信息等。

### 内容管理

- **图标列表**：查询并使用系统图标，优化网站设计。
- **文章管理**：创建、修改、删除文章，支持批量上传图片。
- **类别管理**：对文章类别进行通用管理。
- **作者管理**：批量导入和导出作者数据的 Excel 表格。

### 个人管理

- **资料修改**：更新个人基础资料和头像。
- **密码修改**：定期更新密码，增强账户安全。
- **登录**：支持账号密码登录，记住账号密码，保留登录时长等功能。

### 数据报表

- **可视化展示**：通过折线图、圆饼图、柱状图等直观展示基本数据和多项指标对比。

---

本项目旨在提供一个高效、实用且美观的内容管理系统解决方案，适用于各种规模的项目和学习研究。欢迎使用和贡献！
