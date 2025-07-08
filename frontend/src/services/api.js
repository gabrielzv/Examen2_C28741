import axios from 'axios'

const API_BASE_URL = 'http://localhost:7134/api'

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json'
  }
})

export const productService = {
  async getAllProducts() {
    try {
      const response = await apiClient.get('/products')
      return response.data
    } catch (error) {
      console.error('Error fetching products:', error)
      throw error
    }
  }
}

export const cartService = {
  async calculateTotal(cart) {
    try {
      const response = await apiClient.post('/cart/calculate-total', cart)
      return response.data
    } catch (error) {
      console.error('Error calculating total:', error)
      throw error
    }
  }
}

export default apiClient
