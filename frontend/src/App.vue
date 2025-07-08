<template>
  <div class="vending-machine">
    <header class="machine-header">
      <h1 class="machine-title">Máquina Expendedora</h1>
      <p class="machine-subtitle">Seleccione su refresco favorito</p>
    </header>

    <!-- Cargando -->
    <div v-if="loading" class="loading">
      Cargando productos...
    </div>

    <!-- Error -->
    <div v-else-if="error" class="error">
      {{ error }}
      <br>
      <button @click="loadProducts" style="margin-top: 10px; padding: 5px 15px; background: white; color: #e74c3c; border: none; border-radius: 5px; cursor: pointer;">
        Reintentar
      </button>
    </div>

    <!-- Productos -->
    <div v-else class="products-grid">
      <ProductCard 
        v-for="product in products" 
        :key="product.id" 
        :product="product"
      />
    </div>
  </div>
</template>

<script>
import { ref, onMounted } from 'vue'
import ProductCard from './components/ProductCard.vue'
import { productService } from './services/api.js'

export default {
  name: 'App',
  components: {
    ProductCard
  },
  setup() {
    const products = ref([])
    const loading = ref(false)
    const error = ref(null)

    const loadProducts = async () => {
      loading.value = true
      error.value = null
      
      try {
        const data = await productService.getAllProducts()
        products.value = data
      } catch (err) {
        error.value = 'Error al cargar los productos.'
        console.error('Error loading products:', err)
      } finally {
        loading.value = false
      }
    }

    onMounted(() => {
      loadProducts()
    })

    return {
      products,
      loading,
      error,
      loadProducts
    }
  }
}
</script>
